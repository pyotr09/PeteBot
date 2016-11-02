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

        readonly static TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;

        public void Nominate(string location, string user)
        {
            location = _textInfo.ToTitleCase(location);
            if (IsVetoed(location)) return;
            if (IsNominated(location))
            {
                Second(location, user);
            }
            else
            {
                _nominations.Add(location);
            }
        }

        public bool IsNominated(string location)
        {
            location = _textInfo.ToTitleCase(location);
            return _nominations.Contains(location);
        }

        public void Second(string location, string user)
        {
            location = _textInfo.ToTitleCase(location);
            if (!IsNominated(location))
            {
                Nominate(location, user);
            }
            else
            {
                _seconds.Add(location);
            }
        }

        public bool IsSeconded(string location)
        {
            location = _textInfo.ToTitleCase(location);
            return _seconds.Contains(location);
        }

        public void Veto(string location, string user)
        {
            location = _textInfo.ToTitleCase(location);
            if (!CanVeto(user)) return;
            _nominations.Remove(location);
            _seconds.Remove(location);
            _vetos.Add(location);
            _vetoers.Add(user);
        }

        public bool IsVetoed(string location)
        {
            location = _textInfo.ToTitleCase(location);
            return _vetos.Contains(location);
        }

        public bool CanVeto(string user)
        {
            return !_vetoers.Contains(user);
        }

        public string Status(string location)
        {
            location = _textInfo.ToTitleCase(location);
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

        public void Remove(string location)
        {
            _nominations.Remove(location);
            _seconds.Remove(location);
            _vetos.Remove(location);

        }
    }
}