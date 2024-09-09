using Enocean.Core.Constants;

namespace Enocean.Core.EEps
{
    public partial class EEP
    {
        public Dictionary<string, Dictionary<string, string>>? DiscoverFromRorg(RORG rorg)
        {
            if (xml_telegrams != null)
            {
                var telegram = xml_telegrams.Telegram.FirstOrDefault(t => (RORG)Utils.FromHexStringToInt(t.Rorg!) == rorg);
                if (telegram != null)
                {
                    return telegram.Profiles.ToDictionary(
                         t => t.Func!,
                         t => t.Profile.ToDictionary(
                               profil => profil.Type!,
                               profil => profil.Description));
                }
            }

            return null;
        }
        public string? DiscoverFromRorg(RORG rorg, int rorgFunc, int rorgFype)
        {
            if (xml_telegrams != null)
            {
                var telegram = xml_telegrams.Telegram.FirstOrDefault(t => (RORG)Utils.FromHexStringToInt(t.Rorg!) == rorg);
                if (telegram != null)
                {
                    var funp = telegram.Profiles.FirstOrDefault(p => Utils.FromHexStringToInt(p.Func!) == rorgFunc);
                    if (funp != null)
                    {
                        return $"{funp.Description}  {funp.Profile.FirstOrDefault(p => Utils.FromHexStringToInt(p.Type) == rorgFype)?.Description}";
                    }
                }
            }

            return null;
        }
    }
}
