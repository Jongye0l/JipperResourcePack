using JALib.Core;
using JALib.Core.Setting;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack;

public class ResourcePackSetting(JAMod mod, JObject jsonObject = null) : JASetting(mod, jsonObject) {
    public float Size = 1;
}