using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class ProfileManager
{
    private static ProfileManager _instance;
    public static ProfileManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ProfileManager();
            }
            return _instance;
        }
    }

    private readonly DatabaseManager _dbManager;
    private List<Profile> _cachedProfiles;

    private ProfileManager()
    {
        _dbManager = DatabaseManager.Instance;
        _cachedProfiles = new List<Profile>();
    }
    
    // Initialize and load profiles from database
    public async Task Initialize()
    {
        Debug.Log("Initializing ProfileManager...");
        _cachedProfiles = await _dbManager.GetAll<Profile>();
        Debug.Log($"Loaded {_cachedProfiles.Count} profiles from database");
    }
    
    // Create a new profile
    public async Task<Profile> CreateProfile(string username)
    {
        Debug.Log($"Creating profile for username: {username}");
        
        // Check if username is already taken
        if (_cachedProfiles.Any(p => p.Username.ToLower() == username.ToLower()))
        {
            Debug.LogError($"Username {username} is already taken");
            throw new System.Exception($"Username {username} is already taken");
        }
        
        // Check max profile limit (10 profiles maximum)
        if (_cachedProfiles.Count >= 10)
        {
            Debug.LogError("Maximum profile limit reached (10)");
            throw new System.Exception("Maximum profile limit reached (10)");
        }
        
        var profile = new Profile(username);
        await _dbManager.Insert(profile);
        
        // Refresh cache
        _cachedProfiles = await _dbManager.GetAll<Profile>();
        
        return profile;
    }
    
    // Get profile by id
    public async Task<Profile> GetProfileById(int id)
    {
        // Check cache first
        var cachedProfile = _cachedProfiles.FirstOrDefault(p => p.Id == id);
        if (cachedProfile != null)
        {
            return cachedProfile;
        }
        
        // If not in cache, get from database
        var profile = await _dbManager.GetById<Profile>(id);
        
        // Update cache if found
        if (profile != null && !_cachedProfiles.Any(p => p.Id == profile.Id))
        {
            _cachedProfiles.Add(profile);
        }
        
        return profile;
    }
    
    // Get profile by username
    public async Task<Profile> GetProfileByUsername(string username)
    {
        // Check cache first
        var cachedProfile = _cachedProfiles.FirstOrDefault(p => 
            p.Username.ToLower() == username.ToLower());
            
        if (cachedProfile != null)
        {
            return cachedProfile;
        }
        
        // If not in cache, get from database
        var profile = await _dbManager.QueryFirstOrDefaultAsync<Profile>(
            "SELECT * FROM Profiles WHERE LOWER(Username) = LOWER(?)", username);
        
        // Update cache if found
        if (profile != null && !_cachedProfiles.Any(p => p.Id == profile.Id))
        {
            _cachedProfiles.Add(profile);
        }
        
        return profile;
    }
    
    // Get all profiles
    public async Task<List<Profile>> GetAllProfiles()
    {
        // Refresh cache to ensure we have the latest data
        _cachedProfiles = await _dbManager.GetAll<Profile>();
        return _cachedProfiles;
    }
    
    // Update profile
    public async Task UpdateProfile(Profile profile)
    {
        await _dbManager.Update(profile);
        
        // Update in cache
        var index = _cachedProfiles.FindIndex(p => p.Id == profile.Id);
        if (index >= 0)
        {
            _cachedProfiles[index] = profile;
        }
        else
        {
            _cachedProfiles.Add(profile);
        }
    }
    
    // Delete profile
    public async Task DeleteProfile(int id)
    {
        await _dbManager.Delete<Profile>(id);
        
        // Remove from cache
        _cachedProfiles.RemoveAll(p => p.Id == id);
    }
    
    // Get leaderboard (sorted by ELO)
    public async Task<List<Profile>> GetLeaderboard()
    {
        // Refresh cache to ensure we have the latest data
        _cachedProfiles = await _dbManager.GetAll<Profile>();
        
        // Return sorted by ELO (descending)
        return _cachedProfiles.OrderByDescending(p => p.Elo).ToList();
    }
}