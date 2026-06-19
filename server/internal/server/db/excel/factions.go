package excel

import (
	"context"
	"database/sql"
	"fmt"
	"server/internal/server/db"
	"slices"
	"strconv"
	"strings"

	"github.com/xuri/excelize/v2"
)

type FactionInput struct {
	path     string
	workbook *excelize.File
	ignore   []string
	queries  *db.Queries
	dbCtx    context.Context
}

func NewFactionInput(path string, queries *db.Queries, ctx context.Context) *FactionInput {
	ignore := []string{"Template", "Data"}
	workbook, err := excelize.OpenFile(path, excelize.Options{})
	if err != nil {
		fmt.Println(err)
	}

	return &FactionInput{
		path:     path,
		workbook: workbook,
		ignore:   ignore,
		queries:  queries,
		dbCtx:    ctx,
	}
}

func (fi *FactionInput) InputData() {
	factions := make(map[string]db.Faction)
	armies := make(map[string]map[string]db.Army)
	units := make(map[string]map[string]db.Unit)
	for _, sheet := range fi.workbook.GetSheetList() {
		if slices.Contains(fi.ignore, sheet) {
			continue
		}
		factions[sheet] = fi.faction(sheet)
		armies[sheet] = fi.Army(sheet)
		units[sheet] = fi.Units(sheet)
		fi.FactionArmy(factions[sheet], armies[sheet])
		fi.ArmyUnit(armies[sheet], units[sheet])
	}
	fi.workbook.SaveAs("Shared\\LoadedData.xlsx")
}

func (fi *FactionInput) faction(sheet string) db.Faction {
	fac, err := fi.queries.GetFaction(fi.dbCtx, sheet)
	desc, _ := fi.workbook.GetCellValue(sheet, "B1")
	if err != nil {
		fac = fi.CreateFaction(sheet, desc)
	}
	if fac.Description.String != desc {
		fi.UpdateFaction(sheet, desc)
		fac.Description.String = desc
	}
	fi.workbook.SetCellValue(sheet, "D1", fac.ID)
	return fac
}

func (fi *FactionInput) CreateFaction(name, description string) db.Faction {
	fac, err := fi.queries.NewFaction(fi.dbCtx, db.NewFactionParams{
		Name: name,
		Description: sql.NullString{
			String: description,
			Valid:  true,
		},
	})
	if err != nil {
		fmt.Println(err)
	}
	return fac
}

func (fi *FactionInput) UpdateFaction(sheet, desc string) {
	err := fi.queries.UpdateFaction(fi.dbCtx, db.UpdateFactionParams{
		Name: sheet,
		Description: sql.NullString{
			String: desc,
			Valid:  true,
		},
	})
	if err != nil {
		fmt.Println(err)
	}
}

func (fi *FactionInput) Army(sheet string) map[string]db.Army {
	var armies = make(map[string]db.Army)
	armyRow := 5
	armyName, err := fi.workbook.GetCellValue(sheet, "K"+fmt.Sprint(armyRow))
	if err != nil {
		fmt.Println(err)
		return armies
	}
	for armyName != "" {
		army, err := fi.queries.GetArmy(fi.dbCtx, armyName)
		desc, _ := fi.workbook.GetCellValue(sheet, "L"+fmt.Sprint(armyRow))
		if err != nil {
			army = fi.CreateArmy(armyName, desc)
		}

		if army.Description.String != desc {
			fi.UpdateArmy(sheet, desc)
			army.Description.String = desc
		}
		fi.workbook.SetCellValue(sheet, "M"+fmt.Sprint(armyRow), army.ID)
		armies[armyName] = army
		armyRow++
		armyName, err = fi.workbook.GetCellValue(sheet, "K"+fmt.Sprint(armyRow))
	}
	return armies
}

func (fi *FactionInput) CreateArmy(name, desc string) db.Army {
	army, err := fi.queries.NewArmy(fi.dbCtx, db.NewArmyParams{
		Name: name,
		Description: sql.NullString{
			String: desc,
			Valid:  true,
		},
	})
	if err != nil {
		fmt.Println(err)
	}
	return army
}

func (fi *FactionInput) UpdateArmy(sheet string, desc string) {
	err := fi.queries.UpdateArmy(fi.dbCtx, db.UpdateArmyParams{
		Name: sheet,
		Description: sql.NullString{
			String: desc,
			Valid:  true,
		},
	})
	if err != nil {
		fmt.Println(err)
	}
}

func (fi *FactionInput) Units(sheet string) map[string]db.Unit {
	var units = make(map[string]db.Unit)
	unitRow := 5
	unitName, err := fi.workbook.GetCellValue(sheet, "A"+fmt.Sprint(unitRow))
	if err != nil {
		fmt.Println(err)
		return nil
	}
	for unitName != "" {
		unit, err := fi.queries.GetUnit(fi.dbCtx, unitName)
		attacks, _ := fi.workbook.GetCellValue(sheet, "B"+fmt.Sprint(unitRow))
		movement, _ := fi.workbook.GetCellValue(sheet, "C"+fmt.Sprint(unitRow))
		maxHP, _ := fi.workbook.GetCellValue(sheet, "D"+fmt.Sprint(unitRow))
		AP, _ := fi.workbook.GetCellValue(sheet, "E"+fmt.Sprint(unitRow))
		speed, _ := fi.workbook.GetCellValue(sheet, "F"+fmt.Sprint(unitRow))
		armies, _ := fi.workbook.GetCellValue(sheet, "G"+fmt.Sprint(unitRow))
		speedInt := SpeedToInt(speed)
		MaxHPInt, _ := strconv.Atoi(maxHP)
		APint, _ := strconv.Atoi(AP)
		if err != nil {
			unit = fi.CreateUnit(unitName, attacks, movement, MaxHPInt, APint, speedInt, armies)
		}
		if unit.Attacks.String != attacks || unit.Movement.String != movement || unit.Maxhp.Int64 != int64(MaxHPInt) || unit.Ap.Int64 != int64(APint) || unit.Speed.Int64 != int64(speedInt) {
			fi.UpdateUnit(unitName, attacks, movement, MaxHPInt, APint, speedInt)
			unit.Attacks.String = attacks
			unit.Movement.String = movement
			unit.Maxhp.Int64 = int64(MaxHPInt)
			unit.Ap.Int64 = int64(APint)
			unit.Speed.Int64 = int64(speedInt)
		}
		fi.workbook.SetCellValue(sheet, "H"+fmt.Sprint(unitRow), unit.ID)
		units[unitName] = unit
		unitRow++
		unitName, err = fi.workbook.GetCellValue(sheet, "A"+fmt.Sprint(unitRow))
	}
	return units
}

func (fi *FactionInput) CreateUnit(name, attacks, movement string, maxHP, AP, speed int, armies string) db.Unit {
	unit, err := fi.queries.NewUnit(fi.dbCtx, db.NewUnitParams{
		Name:     name,
		Attacks:  db.NewNullString(attacks),
		Movement: db.NewNullString(movement),
		Maxhp:    db.NewNullInt64(int64(maxHP)),
		Ap:       db.NewNullInt64(int64(AP)),
		Speed:    db.NewNullInt64(int64(speed)),
		Armies:   db.NewNullString(armies),
	})
	if err != nil {
		fmt.Println(err)
	}
	return unit
}

func (fi *FactionInput) UpdateUnit(name, attacks, movement string, maxHP, AP, speed int) {
	err := fi.queries.UpdateUnit(fi.dbCtx, db.UpdateUnitParams{
		Attacks:  db.NewNullString(attacks),
		Movement: db.NewNullString(movement),
		Maxhp:    db.NewNullInt64(int64(maxHP)),
		Ap:       db.NewNullInt64(int64(AP)),
		Speed:    db.NewNullInt64(int64(speed)),
		Name:     name,
	})
	if err != nil {
		fmt.Println(err)
	}
}

func (fi *FactionInput) FactionArmy(faction db.Faction, armies map[string]db.Army) {
	for _, army := range armies {
		_, err := fi.queries.GetArmyFaction(fi.dbCtx, db.GetArmyFactionParams{
			Armyid:    army.ID,
			Factionid: faction.ID,
		})
		if err != nil {
			_, err = fi.queries.NewArmyFaction(fi.dbCtx, db.NewArmyFactionParams{
				Armyid:    army.ID,
				Factionid: faction.ID,
			})
			if err != nil {
				fmt.Println(err)
			}
		}
	}
}

func (fi *FactionInput) ArmyUnit(armies map[string]db.Army, units map[string]db.Unit) {
	for _, unit := range units {
		armiesSplit := strings.Split(unit.Armies.String, ",")
		for _, name := range armiesSplit {
			army := armies[name]
			_, err := fi.queries.GetUnitArmy(fi.dbCtx, db.GetUnitArmyParams{
				Armyid: army.ID,
				Unitid: unit.ID,
			})
			if err != nil {
				_, err = fi.queries.NewUnitArmy(fi.dbCtx, db.NewUnitArmyParams{
					Armyid: army.ID,
					Unitid: unit.ID,
				})
			}
		}
	}
}

func SpeedToInt(speed string) int {
	switch speed {
	case "Normal":
		return 0
	case "Fast":
		return 1
	case "Slow":
		return 2
	}
	return 0
}
