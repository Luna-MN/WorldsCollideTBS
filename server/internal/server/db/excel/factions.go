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
	skills := make(map[string]map[string]db.Skill)
	movements := make(map[string]map[string]db.Movement)
	factions := make(map[string]db.Faction)
	armies := make(map[string]map[string]db.Army)
	units := make(map[string]map[string]db.Unit)
	skills["Data"] = fi.skills("Data")
	movements["Data"] = fi.Movement("Data")
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

func (fi *FactionInput) skills(sheet string) map[string]db.Skill {
	skillRow := 4
	skills := make(map[string]db.Skill)

	skillName, err := fi.workbook.GetCellValue(sheet, "A"+fmt.Sprint(skillRow))
	for skillName != "" {

		skillName, err = fi.workbook.GetCellValue(sheet, "A"+fmt.Sprint(skillRow))
		if err != nil || skillName == "" {
			break
		}
		skillName = strings.Replace(skillName, " ", "", -1)
		skill, err := fi.queries.GetSkillByName(fi.dbCtx, skillName)

		CS, _ := fi.workbook.GetCellValue(sheet, "B"+fmt.Sprint(skillRow))
		A, _ := fi.workbook.GetCellValue(sheet, "C"+fmt.Sprint(skillRow))
		AP, _ := strconv.Atoi(A)
		R, _ := fi.workbook.GetCellValue(sheet, "D"+fmt.Sprint(skillRow))
		Range, _ := strconv.Atoi(R)
		Desc, _ := fi.workbook.GetCellValue(sheet, "E"+fmt.Sprint(skillRow))
		U, _ := fi.workbook.GetCellValue(sheet, "F"+fmt.Sprint(skillRow))
		Universal, _ := strconv.ParseBool(U)
		Type, _ := fi.workbook.GetCellValue(sheet, "G"+fmt.Sprint(skillRow))
		C, _ := fi.workbook.GetCellValue(sheet, "H"+fmt.Sprint(skillRow))
		Cooldown, _ := strconv.Atoi(C)

		if err != nil {
			skill, err = fi.queries.NewSkill(fi.dbCtx, db.NewSkillParams{
				Name:         skillName,
				Description:  Desc,
				Type:         Type,
				Cooldown:     int64(Cooldown),
				Ap:           int64(AP),
				Range:        int64(Range),
				Combatstring: sql.NullString{String: CS, Valid: true},
				Universal:    sql.NullBool{Bool: Universal, Valid: true},
			})
		}
		if skill.Combatstring.String != CS || skill.Ap != int64(AP) || skill.Range != int64(Range) || skill.Description != Desc || skill.Type != Type || skill.Cooldown != int64(Cooldown) || skill.Universal.Bool != Universal {
			err = fi.queries.UpdateSkill(fi.dbCtx, db.UpdateSkillParams{
				Name:         skillName,
				Description:  Desc,
				Type:         Type,
				Cooldown:     int64(Cooldown),
				Ap:           int64(AP),
				Range:        int64(Range),
				Combatstring: sql.NullString{String: CS, Valid: true},
				Universal:    sql.NullBool{Bool: Universal, Valid: true},
			})
		}

		skills[skillName] = skill
		skillRow++
	}

	return skills
}

func (fi *FactionInput) Movement(sheet string) map[string]db.Movement {
	moveRow := 4
	movements := make(map[string]db.Movement)

	movementName, err := fi.workbook.GetCellValue(sheet, "K"+fmt.Sprint(moveRow))
	for movementName != "" {
		movementName, err = fi.workbook.GetCellValue(sheet, "K"+fmt.Sprint(moveRow))
		if err != nil || movementName == "" {
			break
		}
		movement, err := fi.queries.GetMovementByName(fi.dbCtx, movementName)

		description, _ := fi.workbook.GetCellValue(sheet, "M"+fmt.Sprint(moveRow))
		moveCost, _ := fi.workbook.GetCellValue(sheet, "L"+fmt.Sprint(moveRow))
		moveCostInt, _ := strconv.Atoi(moveCost)
		if err != nil {
			movement, err = fi.queries.NewMovement(fi.dbCtx, db.NewMovementParams{
				Name:        movementName,
				Description: description,
				Movecost:    int64(moveCostInt),
			})
		}
		if movement.Description != description || movement.Movecost != int64(moveCostInt) {
			err = fi.queries.UpdateMovement(fi.dbCtx, db.UpdateMovementParams{
				Name:        movementName,
				Description: description,
				Movecost:    int64(moveCostInt),
			})
		}

		movements[movementName] = movement
		moveRow++
	}
	return movements
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
		skillsUnformatted, _ := fi.workbook.GetCellValue(sheet, "B"+fmt.Sprint(unitRow))

		skillsSplit := strings.Split(skillsUnformatted, ",")
		skillNames := make([]string, len(skillsSplit))
		for i, skill := range skillsSplit {
			skillNames[i] = strings.Replace(skill, " ", "", -1)
		}
		skills := strings.Join(skillNames, ",")
		movement, _ := fi.workbook.GetCellValue(sheet, "C"+fmt.Sprint(unitRow))
		maxHP, _ := fi.workbook.GetCellValue(sheet, "D"+fmt.Sprint(unitRow))
		AP, _ := fi.workbook.GetCellValue(sheet, "E"+fmt.Sprint(unitRow))
		speed, _ := fi.workbook.GetCellValue(sheet, "F"+fmt.Sprint(unitRow))
		armies, _ := fi.workbook.GetCellValue(sheet, "G"+fmt.Sprint(unitRow))
		support, _ := fi.workbook.GetCellValue(sheet, "I"+fmt.Sprint(unitRow))
		speedInt := SpeedToInt(speed)
		MaxHPInt, _ := strconv.Atoi(maxHP)
		APint, _ := strconv.Atoi(AP)
		if err != nil {
			unit = fi.CreateUnit(unitName, skills, movement, support, MaxHPInt, APint, speedInt, armies)
		}
		if unit.Skills.String != skills || unit.Movement.String != movement || unit.Maxhp.Int64 != int64(MaxHPInt) || unit.Ap.Int64 != int64(APint) || unit.Speed.Int64 != int64(speedInt) || unit.Armies.String != armies {
			fi.UpdateUnit(unitName, skills, movement, support, MaxHPInt, APint, speedInt, armies)
			unit.Skills.String = skills
			unit.Movement.String = movement
			unit.Maxhp.Int64 = int64(MaxHPInt)
			unit.Ap.Int64 = int64(APint)
			unit.Speed.Int64 = int64(speedInt)
			unit.Armies.String = armies
		}
		fi.workbook.SetCellValue(sheet, "H"+fmt.Sprint(unitRow), unit.ID)
		units[unitName] = unit
		unitRow++
		unitName, err = fi.workbook.GetCellValue(sheet, "A"+fmt.Sprint(unitRow))
	}
	return units
}

func (fi *FactionInput) CreateUnit(name, skills, movement, support string, maxHP, AP, speed int, armies string) db.Unit {
	unit, err := fi.queries.NewUnit(fi.dbCtx, db.NewUnitParams{
		Name:     name,
		Skills:   db.NewNullString(skills),
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

func (fi *FactionInput) UpdateUnit(name, skills, movement, support string, maxHP, AP, speed int, armies string) {
	err := fi.queries.UpdateUnit(fi.dbCtx, db.UpdateUnitParams{
		Skills:   db.NewNullString(skills),
		Movement: db.NewNullString(movement),
		Maxhp:    db.NewNullInt64(int64(maxHP)),
		Ap:       db.NewNullInt64(int64(AP)),
		Speed:    db.NewNullInt64(int64(speed)),
		Armies:   db.NewNullString(armies),
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
			armySplit := strings.Split(name, " ")
			army := armies[armySplit[0]]
			Count := 1
			if len(armySplit) > 1 {
				Count, _ = strconv.Atoi(armySplit[1])
			}
			if Count == 0 {
				Count = 1
			}
			ua, err := fi.queries.GetUnitArmy(fi.dbCtx, db.GetUnitArmyParams{
				Armyid: army.ID,
				Unitid: unit.ID,
			})
			if err != nil {
				ua, err = fi.queries.NewUnitArmy(fi.dbCtx, db.NewUnitArmyParams{
					Armyid: army.ID,
					Unitid: unit.ID,
					Count:  int64(Count),
				})
			}
			if ua.Count != int64(Count) {
				err = fi.queries.UpdateUnitArmy(fi.dbCtx, db.UpdateUnitArmyParams{
					Armyid: army.ID,
					Unitid: unit.ID,
					Count:  int64(Count),
				})
				if err != nil {
					fmt.Println(err)
				}
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
