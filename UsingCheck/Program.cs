global using System.Text.Json;

TraverseTree("C:\\Dev\\VapianoPortal\\src");

Console.WriteLine("Press any key");
Console.ReadKey();

static void TraverseTree(string root)
{
    
    // Data structure to hold names of subfolders to be
    // examined for files.
    Stack<string> dirs = new Stack<string>();

    var usingsStatistic = new List<UsingCount>();

    if (!Directory.Exists(root))
    {
        throw new ArgumentException("Folder does not exists.");
    }

    dirs.Push(root);

    while (dirs.Count > 0)
    {
        if (dirs.Contains("frontend"))
        {
            continue;
        }
        string currentDir = dirs.Pop();
        string[] subDirs;
        try
        {
            subDirs = Directory.GetDirectories(currentDir);
        }
        // An UnauthorizedAccessException exception will be thrown if we do not have
        // discovery permission on a folder or file. It may or may not be acceptable
        // to ignore the exception and continue enumerating the remaining files and
        // folders. It is also possible (but unlikely) that a DirectoryNotFound exception
        // will be raised. This will happen if currentDir has been deleted by
        // another application or thread after our call to Directory.Exists. The
        // choice of which exceptions to catch depends entirely on the specific task
        // you are intending to perform and also on how much you know with certainty
        // about the systems on which this code will run.
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }

        string[] files = null;
        try
        {
            files = Directory.GetFiles(currentDir);
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine(e.Message);
            continue;
        }
        // Perform the required action on each file here.
        // Modify this block to perform your required task.

        var progressIndicator = 0;
        foreach (string file in files)
        {
            if (file.Contains("Migrations") || file.Contains("bin") || file.Contains("obj"))
            {
                continue;
            }
            progressIndicator++;
            Console.WriteLine($"Progress: { progressIndicator } / {files.Count()}");
            var usingsInFile = new List<string>();

            try
            {
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    if (line.StartsWith("using"))
                    {
                        usingsInFile.Add(line);
                    }
                }

                foreach (var singleUsing in usingsInFile)
                {
                    var existingUsing = usingsStatistic.FirstOrDefault(x => x.UsingName.Equals(singleUsing));
                    if (existingUsing is not null)
                    {
                        existingUsing.Count++;
                    }
                    else
                    {
                        usingsStatistic.Add(new UsingCount(singleUsing));
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                // If file was deleted by a separate application
                //  or thread since the call to TraverseTree()
                // then just continue.
                Console.WriteLine(e.Message);
                continue;
            }
        }

        usingsStatistic = usingsStatistic.OrderByDescending(x => x.Count).ToList();

        // Push the subdirectories onto the stack for traversal.
        // This could also be done before handing the files.
        foreach (string str in subDirs)
            dirs.Push(str);
    }

    var json = JsonSerializer.Serialize(usingsStatistic);
    File.WriteAllText("Usings.json", json);
}

class UsingCount
{
    public UsingCount(string usingName)
    {
        UsingName = usingName;
        Count = 1;
    }

    public string UsingName { get; set; }
    public int Count { get; set; }
}

//Delete empty regions regex: #region Usings\n\n#endregion\n\n