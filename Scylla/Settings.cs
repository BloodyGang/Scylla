using System.IO.Compression;

class Settings
{
    public void PackFilesToCfg(string sourceFolderPath, string cfgFilePath)
    {
        // Get a list of files in the specified folder
        string[] files = Directory.GetFiles(sourceFolderPath);

        // Create a new .cfg file
        using (FileStream cfgFileStream = new FileStream(cfgFilePath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(cfgFileStream, ZipArchiveMode.Create))
            {
                // Add each file to the archive
                foreach (string filePath in files)
                {
                    // Get the file name including path
                    string fileName = Path.GetFileName(filePath);

                    // Add file to the archive
                    archive.CreateEntryFromFile(filePath, fileName);
                }
            }
        }

        Console.WriteLine("Files have been packed into the .cfg file.");
    }

    public void UnpackFilesFromCfg(string cfgFilePath, string destinationFolderPath)
    {
        // Check the existence of the .cfg file
        if (!File.Exists(cfgFilePath))
        {
            Console.WriteLine("The .cfg file does not exist.");
            return;
        }

        // Create the target folder for unpacking files
        Directory.CreateDirectory(destinationFolderPath);

        // Open the .cfg file for reading
        using (FileStream cfgFileStream = new FileStream(cfgFilePath, FileMode.Open))
        {
            using (ZipArchive archive = new ZipArchive(cfgFileStream, ZipArchiveMode.Read))
            {
                // Unpack each file from the archive to the target folder
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Create the target file name (possibly overwrites an existing file)
                    string destinationFilePath = Path.Combine(destinationFolderPath, entry.FullName);

                    // Unpack the file
                    entry.ExtractToFile(destinationFilePath, true);
                }
            }
        }

        Console.WriteLine("Files have been successfully unpacked from the .cfg file.");
    }
}
