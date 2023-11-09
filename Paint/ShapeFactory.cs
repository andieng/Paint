using Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Paint
{
    public class ShapeFactory
    {
        private Dictionary<string, IShape> _prototypes;
        private static ShapeFactory _instance = null;

        private ShapeFactory()
        {
            _prototypes = new Dictionary<string, IShape>();
            loadDlls();

        }

        public static ShapeFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ShapeFactory();
                }
                return _instance;
            }
        }

        public Dictionary<string, IShape> GetPrototypes()
        {
            return _prototypes;
        }

        public IShape Create(string shapeName)
        {
            IShape shape = null;
            if (_prototypes.ContainsKey(shapeName))
            {
                shape = _prototypes[shapeName].Clone();
            }
            return shape;
        }

        private void loadDlls()
        {
            var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            var dlls = new DirectoryInfo(exeFolder).GetFiles("*.dll");

            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (typeof(IShape).IsAssignableFrom(type))
                        {
                            var shape = Activator.CreateInstance(type) as IShape;
                            if (shape != null)
                            {
                                if (!_prototypes.ContainsKey(shape.Name))
                                {
                                    _prototypes.Add(shape.Name, shape);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
