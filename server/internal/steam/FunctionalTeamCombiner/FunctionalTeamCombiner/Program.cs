
using System.Collections;
using Spire.Xls;

Console.WriteLine("Enter a directory");
var dir = Console.ReadLine();
File.Copy("C:\\Users\\matt\\Documents\\worlds-collide-tbs\\server\\internal\\steam\\FunctionalTeamCombiner\\FunctionalTeamCombiner\\RTU Functional Individual Project Status - Template.xlsx", $"{dir}\\RTU Functional Individual Project Status - Combined.xlsx");
var OutputWorkbook = new Workbook();
OutputWorkbook.LoadFromFile($"{dir}\\RTU Functional Individual Project Status - Combined.xlsx");
var outputWorksheet = OutputWorkbook.Worksheets[0];

var startCell = outputWorksheet.Rows[5].Cells[2];
if (startCell.Value != "Project Number")
{
    Console.WriteLine($"Errored on combined, template incorrect");
    return;
}
var outputDict = CreateDictionary(outputWorksheet.Rows[5]);
var startRow = 6;
var currOutputRow = 6;
var format = outputWorksheet.Rows[6].Cells[3].Style;

foreach (var file in Directory.GetFiles(dir))
{
    if (file.EndsWith(".xlsx") && !file.Contains("Combined"))
    {
        var data = GetDataFromExcel(file);
        if (data == null)
        {
            continue;
        }

        for (int i = 0; i < data.Count; i++)
        {
            outputWorksheet.InsertRow(currOutputRow+i+1);
            outputWorksheet.Rows[currOutputRow+i].Cells[1].Style = format;
            foreach (var col in outputDict.Values)
            {
                outputWorksheet.Rows[currOutputRow+i].Cells[col].Style = format;
            }
        }
        foreach (var d in data)
        {
            outputWorksheet.Rows[currOutputRow].Cells[1].Value = d.EngineerName;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Project Number"]].Value = d.ProjectNumber;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Project Name"]].Value = d.ProjectName;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Client / Framework"]].Value = d.Client;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Project Manager"]].Value = d.ProjectManager;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Current Phase"]].Value = d.CurrentPhase;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Next Key Milestone"]].Value = d.NextKeyMilestone;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Milestone Date"]].Value = d.MilestoneDate;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["RAG Status"]].Value = d.RAGStatus;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Blockers (high level)"]].Value = d.Blockers;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Escalations (high level)"]].Value = d.Escalations;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Dependencies (high level)"]].Value = d.Dependencies;
            outputWorksheet.Rows[currOutputRow].Cells[outputDict["Planned Leave Before Next Milestone"]].Value = d.PlannedLeaveBeforeNextMilestone;

            currOutputRow++;
        }

        
    }
}
OutputWorkbook.Save();
List<Data> GetDataFromExcel(string file)
{
    var Workbook = new Workbook();
    Workbook.LoadFromFile(file);
    if (Workbook.Worksheets.Count < 0)
    {
        Console.WriteLine($"Errored on {file}");
        return null;
    }
    var worksheet = Workbook.Worksheets[0];
    var startCell = worksheet.Rows[5].Cells[1];
    if (startCell.Value != "Project Number")
    {
        Console.WriteLine($"Errored on {file}, template incorrect");
        Console.WriteLine(startCell.Value);
    }
    
    var dict = CreateDictionary(worksheet.Rows[5]);
    var Data = new List<Data>();
    
    var engineerName = file.Split('\\').Last().Split('-')[1];
    engineerName = engineerName.Trim();
    var engineerSplit = engineerName.Split(' ');
    var engineerFirstName = engineerSplit[0];
    var engineerLastName = engineerSplit[1].Replace(".xlsx", "");
    var engineerNameFull = $"{engineerFirstName} {engineerLastName}";
    Console.WriteLine($"Processing {engineerNameFull}");
    for (int i = 6; i < 9999; i++)
    {
        if (worksheet.Rows[i].Cells[dict["Project Name"]].Value == "")
        {
            break;
        }
        var PN = GetCell(worksheet, i, dict["Project Number"]);
        var PName = GetCell(worksheet, i, dict["Project Name"]);
        var Client = GetCell(worksheet, i, dict["Client / Framework"]);
        var PM = GetCell(worksheet, i, dict["Project Manager"]);
        var Phase = GetCell(worksheet, i, dict["Current Phase"]);
        var NextKeyMilestone = GetCell(worksheet, i, dict["Next Key Milestone"]);
        var MilestoneDate = GetCell(worksheet, i, dict["Milestone Date"]);
        var RAGStatus = GetCell(worksheet, i, dict["RAG Status"]);
        var Blockers = GetCell(worksheet, i, dict["Blockers (high level)"]);
        var Escalations = GetCell(worksheet, i, dict["Escalations (high level)"]);
        var Dependencies = GetCell(worksheet, i, dict["Dependencies (high level)"]);
        var PlannedLeaveBeforeNextMilestone = GetCell(worksheet, i, dict["Planned Leave Before Next Milestone"]);
        Data.Add(new Data(engineerNameFull, PN, PName, Client, PM, Phase, NextKeyMilestone, MilestoneDate, RAGStatus, Blockers, Escalations, Dependencies, PlannedLeaveBeforeNextMilestone));
    }
    return Data;
}

Dictionary<string, int> CreateDictionary(CellRange row)
{
    var started = false;
    var dictionary = new Dictionary<string, int>();
    foreach (var cell in row.Cells)
    {
        if(!started && cell.Value == "Project Number")
        {
            started = true;
            dictionary.Add(cell.Value, row.Cells.IndexOf(cell));
            continue;
        }
        if (started)
        {
            dictionary.Add(cell.Value, row.Cells.IndexOf(cell));
            if (cell.Value == "Planned Leave Before Next Milestone")
            {
                break;
            }
        }
    }
    return dictionary;
}

string GetCell(Worksheet worksheet, int row, int col)
{
    return worksheet.Rows[row].Cells[col].Value ?? "";
}

class Data(string engineerName, string projectNumber, string projectName, string client, string projectManager, string currentPhase, string nextKeyMilestone, string milestoneDate, string RAGStatus, string blockers, string escalations, string dependencies, string plannedLeaveBeforeNextMilestone)
{
    public string EngineerName = engineerName;
    public string ProjectNumber = projectNumber;
    public string ProjectName = projectName;
    public string Client = client;
    public string ProjectManager = projectManager;
    public string CurrentPhase = currentPhase;
    public string NextKeyMilestone = nextKeyMilestone;
    public string MilestoneDate = milestoneDate;
    public string RAGStatus = RAGStatus;
    public string Blockers = blockers;
    public string Escalations = escalations;
    public string Dependencies = dependencies;
    public string PlannedLeaveBeforeNextMilestone = plannedLeaveBeforeNextMilestone;
}