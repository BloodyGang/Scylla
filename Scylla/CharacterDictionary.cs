using Newtonsoft.Json;

public static class CharacterDictionaries
{
    private static Dictionary<string, string> _killersDict;
    private static Dictionary<string, string> _survivorsDict;
    private static Dictionary<string, string> _charactersDict;
    private static bool _isInitialized = false;

    private static string KillersFile = Path.Combine("ProfileFiles", "killer.json");
    private static string SurvivorsFile = Path.Combine("ProfileFiles", "survivor.json");
    private static string CharactersFile = Path.Combine("ProfileFiles", "character.json");

    public static void Initialize()
    {
        if (!_isInitialized)
        {
            _killersDict = InitializeDictionary(KillersFile);
            _survivorsDict = InitializeDictionary(SurvivorsFile);
            _charactersDict = InitializeDictionary(CharactersFile);
            _isInitialized = true;
        }
    }

    private class Character
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Difficulty { get; set; }
        public string Gender { get; set; }
        public string Height { get; set; }
        public string Bio { get; set; }
        public string Story { get; set; }
        public object Tunables { get; set; }
        public object Item { get; set; }
        public List<string> Outfit { get; set; }
        public object Dlc { get; set; }
        public string Image { get; set; }
    }

    private static Dictionary<string, string> InitializeDictionary(string filePath)
    {
        var jsonContent = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonContent);

        if (filePath == CharactersFile)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(CharactersFile, json);
        }

        var items = new List<KeyValuePair<string, string>>();

        foreach (var kvp in data)
        {
            var character = kvp.Value;
            var id = (string)character.id;
            var name = "";
            if ((string)character.role == "survivor")
            {
                name = ((string)character.name);
            }
            else
            {
                name = ((string)character.name).Replace("The", "").Trim();
            }
            items.Add(new KeyValuePair<string, string>(id, name));
        }

        items.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        var sortedDictionary = new Dictionary<string, string>();
        foreach (var item in items)
        {
            sortedDictionary.Add(item.Key, item.Value);
        }

        return sortedDictionary;
    }

    public static string TranslateKiller(string id)
    {
        if (_killersDict != null && _killersDict.TryGetValue(id, out var name))
        {
            return name.Replace("The", "");
        }
        return "@New Killer";
    }

    public static string TranslateSurvivor(string id)
    {
        if (_survivorsDict != null && _survivorsDict.TryGetValue(id, out var name))
        {
            return name;
        }
        return "@New Survivor";
    }

    public static string TranslateCharacter(string id)
    {
        static string? FindKeyById(Dictionary<string, Character> characters, string id)
        {
            foreach (var entry in characters)
            {
                if (entry.Value.Id == id)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        string characterString = File.ReadAllText(CharactersFile);
        var characters = JsonConvert.DeserializeObject<Dictionary<string, Character>>(characterString);
        string keyFound = FindKeyById(characters, id);

        if (_charactersDict != null && _charactersDict.TryGetValue(id, out var name))
        {
            try
            {
                if (characters[keyFound].Role == "survivor")
                {
                    return name;
                }
                else
                {
                    return name.Replace("The", "");
                }
            }
            catch (ArgumentNullException)
            {
                return "@New Char";
            }
        }
        return "@New Char";
    }
}
