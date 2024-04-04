using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace cCoder.Core.Objects.Dtos
{
    public class DynamicXml : DynamicObject
    {
        private readonly XElement _root;
        private DynamicXml(XElement root) => _root = root;

        public static DynamicXml Parse(string xmlString) => new(XDocument.Parse(xmlString).Root);

        public static DynamicXml Load(string filename) => new(XDocument.Load(filename).Root);

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            XAttribute att = _root.Attribute(binder.Name);
            if (att != null)
            {
                result = att.Value;
                return true;
            }

            System.Collections.Generic.IEnumerable<XElement> nodes = _root.Elements(binder.Name);
            if (nodes.Count() > 1)
            {
                result = nodes.Select(n => n.HasElements ? (object)new DynamicXml(n) : n.Value).ToList();
                return true;
            }

            XElement node = _root.Element(binder.Name);
            if (node != null)
            {
                result = node.HasElements ? new DynamicXml(node) : node.Value;
                return true;
            }

            return true;
        }
    }
}
