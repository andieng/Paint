﻿using Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;

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


        public IShape Create(string shapeName, Color colorStroke, Color colorFill, int strokeSize, double[]? strokeDashArray, string textContent = "")
        {
            if (!_prototypes.ContainsKey(shapeName))
                return null;

            var shape = _prototypes[shapeName].Create();
            shape.ColorStroke = new SolidColorBrush(colorStroke);
            shape.ColorFill = new SolidColorBrush(colorFill);
            shape.StrokeSize = strokeSize;
            shape.TextContent = textContent;
            if (strokeDashArray != null)
            {
                shape.StrokeDashArray = strokeDashArray;
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
