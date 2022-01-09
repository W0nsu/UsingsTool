#region Usings

using GemBox.Spreadsheet;
using UsingCheck.Helpers;

#endregion

const string solutionPath = "/Users/wonsu/Repositories/UsingCheck/TestProject";
GenerateUsingsStatistics(solutionPath);

Console.WriteLine("Press any key");
Console.ReadKey();

static void GenerateUsingsStatistics(string solutionPath)
{
	if (!Directory.Exists(solutionPath))
		throw new ArgumentException("Folder does not exists.");

	var directoriesToCheck = new Stack<string>();
	directoriesToCheck.Push(solutionPath);

	var usingStats = new List<UsingCount>();

	var spinner = new ConsoleSpinner();
	Console.WriteLine("Processing...");
	while (directoriesToCheck.Count > 0)
	{
		spinner.Turn();
		var currentDirectory = directoriesToCheck.Pop();

		GetSubDirectoriesFromDirectory(currentDirectory, out var subDirectories);
		if (subDirectories is null)
			continue;

		foreach (var directory in subDirectories)
			directoriesToCheck.Push(directory);

		GetFilesFromDirectory(currentDirectory, out var files);

		if (files is null)
			continue;

		foreach (var file in files)
		{
			var usingsInFile = GetUsingsFromFile(file);
			if (usingsInFile is null)
				continue;

			AddUsingsFromFileToStatistics(usingsInFile, usingStats);
		}
	}

	usingStats = usingStats.OrderByDescending(x => x.Count).ToList();
	GenerateXlsx(usingStats);
}

static void GetSubDirectoriesFromDirectory(string path, out string[]? subDirectories)
{
	try
	{
		subDirectories = Directory.GetDirectories(path);
		return;
	}
	catch (UnauthorizedAccessException e)
	{
		Console.WriteLine(e.Message);
	}
	catch (DirectoryNotFoundException e)
	{
		Console.WriteLine(e.Message);
	}

	subDirectories = null;
}

static void GetFilesFromDirectory(string path, out string[]? files)
{
	try
	{
		files = Directory.GetFiles(path);
		return;
	}
	catch (UnauthorizedAccessException e)
	{
		Console.WriteLine(e.Message);
	}
	catch (DirectoryNotFoundException e)
	{
		Console.WriteLine(e.Message);
	}

	files = null;
}

static List<string>? GetUsingsFromFile(string filePath)
{
	if (IsFileOmitted(filePath))
		return null;

	try
	{
		return ReadLinesAndReturnUsings(filePath);
	}
	catch (FileNotFoundException e)
	{
		Console.WriteLine(e.Message);
	}

	return null;
}

static bool IsFileOmitted(string filePath)
{
	return filePath.Contains("Migrations") || filePath.Contains("bin") || filePath.Contains("obj");
}

static List<string> ReadLinesAndReturnUsings(string filePath)
{
	var lines = File.ReadAllLines(filePath);
	var usingsInFile = new List<string>();
	foreach (var line in lines)
	{
		if (line.StartsWith("using"))
		{
			usingsInFile.Add(line);
			continue;
		}

		if (line.StartsWith("namespace"))
			break;
	}

	return usingsInFile;
}

static void AddUsingsFromFileToStatistics(List<string> usingsInFile, ICollection<UsingCount> usingStats)
{
	foreach (var singleUsing in usingsInFile)
	{
		var existingUsing = usingStats.FirstOrDefault(x => x.UsingName.Equals(singleUsing));
		if (existingUsing is not null)
			existingUsing.Count++;
		else
			usingStats.Add(new UsingCount(singleUsing));
	}
}

static void GenerateXlsx(List<UsingCount> usingStats)
{
	SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

	var workbook = new ExcelFile();
	var worksheet = workbook.Worksheets.Add("Usings");

	worksheet.Cells[0, 0].Value = "Using name";
	worksheet.Cells[0, 1].Value = "Count";

	var row = 1;
	foreach (var usingStat in usingStats)
	{
		worksheet.Cells[row, 0].Value = usingStat.UsingName;
		worksheet.Cells[row, 1].Value = usingStat.Count;
		row++;
	}

	workbook.Save("../../../Usings.xlsx");
}

//Delete empty regions regex: #region Usings\n\n#endregion\n\n