using System.Collections.Frozen;
using TagLib;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Album Art Ripper");
        if (!args.Any())
        {
            Console.WriteLine("No directory has been provided. The application will now exit.");
            Environment.Exit(1);
        }

        string searchDir = args[0];
        int dirsScanned = 0;
        List<string> badDirs = new();
        try
        {
            // Artist
            foreach (string parent in Directory.GetDirectories(searchDir))
            {
                //Album
                foreach (string child in Directory.GetDirectories(parent))
                {
                    dirsScanned++;
                    bool jpgPresent = false;
                    foreach (string file in Directory.GetFiles(child, "folder.jpg"))
                    {
                        if (jpgPresent == false)
                        {
                            string extension = Path.GetExtension(file);
                            if (extension != null && (extension.Equals(".jpg")))
                            {
                                jpgPresent = true;
                                Console.Write(".");
                            }
                        }

                    }
                    if (jpgPresent == false)
                    {
                        badDirs.Add(child);
                        Console.Write('X');
                    }
                }
            }
            if (badDirs.Any())
            {
                List<string> orderedBadDirs = badDirs.Where(s => !s.Contains(".stversions")).Order().ToList();
                string logFileName = $"MissingArt-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.log";
                var logFile = System.IO.File.CreateText(logFileName);

                logFile.WriteLine($"{orderedBadDirs.Count()} / {dirsScanned} scanned albums have missing \"folder.jpg\" files. These are:");
                logFile.WriteLine();
                foreach (string dir in orderedBadDirs)
                {
                    logFile.WriteLine(dir);
                }
                logFile.Close();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"{orderedBadDirs.Count()} albums have missing \"folder.jpg\" files.");
                Console.WriteLine($"For more information, view the {logFileName} file.");
                Console.WriteLine();

                // Retrieve & Save Artwork.
                foreach (string dir in orderedBadDirs)
                {
                    bool albumProcessed = false;
                    List<string> files = Directory.GetFiles(dir).ToList();
                    foreach (string file in files)
                    {
                        if (albumProcessed == false)
                        {
                            var tfile = TagLib.File.Create(file);
                            if (tfile.Tag.Pictures.Any())
                            {
                                var albumart = tfile.Tag.Pictures.FirstOrDefault();
                                var toSave = albumart.Data;
                                var newFile = System.IO.File.Create($"{dir}/folder.jpg");

                                MemoryStream ms = new();
                                byte[] bytes = toSave.ToArray();
                                newFile.Write(bytes);
                                newFile.Close();
                                albumProcessed = true;
                                Console.WriteLine($"Saved {newFile.Name}");
                            }
                            else
                            {
                                Console.WriteLine($"No album art for {file}!");
                            }
                        }
                    }

                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("No Issues Found. :¬)");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An exception occurred: {e.Message}");
            Console.WriteLine(e.StackTrace);
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("The application will now exit.");
        }
    }
}