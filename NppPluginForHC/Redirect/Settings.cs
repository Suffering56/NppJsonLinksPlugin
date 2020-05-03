namespace NppPluginForHC.Redirect
{
    public class MappingSettings
    {
        // D:/projects/shelter/gd_data/festivalGoods.json:[]:extension:rewardPackId==>D:/projects/shelter/gd_data/rewardPacks.json:[]:id
    }

    class Item
    {
        private string filePath; // = "D:/projects/shelter/gd_data/festivalGoods.json";
        private Element rootElement;
    }

    class Element
    {
        private readonly string _name;
        private readonly Element _child;

        public Element(string name, Element child)
        {
            this._name = name;
            this._child = child;
        }
    }
}