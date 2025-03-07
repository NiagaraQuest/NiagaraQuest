using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ProfileManager
{
    private static ProfileManager _instance;
    private readonly DatabaseManager dbManager;
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
    private ProfileManager()
    {
        dbManager = DatabaseManager.Instance;
    }

    public async Task<bool> CreateProfile(string username)
    {
        Profile newProfile = new Profile(username);
        
        if (newProfile.Id == 0)
        {
            Debug.LogError("Failed to create profile.");
            return false;
        }

        await dbManager.Insert(newProfile);
        Debug.Log($"Profile {username} created with ID: {newProfile.Id}");
        return true;
    }

    public async Task<Profile> GetProfileById(int id)
    {
        return await dbManager.GetById<Profile>(id);
    }

    public async Task<List<Profile>> GetAllProfiles()
    {
        return await dbManager.GetAll<Profile>();
    }

    public async Task<bool> UpdateProfile(Profile profile)
    {
        if (profile == null)
        {
            Debug.LogError("Profile is null, cannot update.");
            return false;
        }

        await dbManager.Update(profile);
        Debug.Log($"Profile {profile.Username} updated.");
        return true;
    }

    public async Task<bool> DeleteProfile(int id)
    {
        await dbManager.Delete<Profile>(id);
        Debug.Log($"Profile with ID {id} deleted.");
        return true;
    }
}
