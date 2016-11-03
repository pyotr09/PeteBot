using System;
using System.Collections.Generic;
using System.Globalization;

namespace LunchBot
{


    [Serializable]
    public class DataStore
    {
        public static DataStore Instance = new DataStore();
        readonly HashSet<string> _nominations = new HashSet<string>();
        readonly HashSet<string> _seconds = new HashSet<string>();
        readonly List<string> _vetos = new List<string>();
        readonly List<string> _vetoers = new List<string>();
        readonly List<string> _adminUsers = new List<string>();

        public void AddRequest(string location, string user)
        {
            location = location.ToTitleCase();
            if (IsVetoed(location)) return;
            if (IsNominated(location))
            {
                _seconds.Add(location);
            }
            else
            {
                _nominations.Add(location);
            }
        }

        public bool IsNominated(string location)
        {
            location = location.ToTitleCase();
            return _nominations.Contains(location);
        }

        public bool IsSeconded(string location)
        {
            location = location.ToTitleCase();
            return _seconds.Contains(location);
        }

        public void Veto(string location, string user)
        {
            location = location.ToTitleCase();
            if (!CanVeto(user)) return;
            _nominations.Remove(location);
            _seconds.Remove(location);
            _vetos.Add(location);
            _vetoers.Add(user);
        }

        public bool IsVetoed(string location)
        {
            location = location.ToTitleCase();
            return _vetos.Contains(location);
        }

        public bool CanVeto(string user)
        {
            return !_vetoers.Contains(user);
        }

        public string Status(string location)
        {
            location = location.ToTitleCase();
            if (IsVetoed(location)) return "vetoed";
            if (IsSeconded(location)) return "seconded";
            if (IsNominated(location)) return "nominated";
            return "nothing currently";
        }

        public List<string> GetNominations()
        {
            return new List<string>(_nominations);
        }

        public List<string> GetSeconds()
        {
            return new List<string>(_seconds);
        }

        public bool Remove(string location, string user)
        {
            if (!_adminUsers.Contains(user)) return false;
            _nominations.Remove(location);
            _seconds.Remove(location);
            _vetos.Remove(location);
            return true;
        }

        public void MakeAdmin(string user)
        {
            _adminUsers.Add(user);
        }

        public string GetAdmins()
        {
            return string.Join(", ", _adminUsers);
        }
    }
}