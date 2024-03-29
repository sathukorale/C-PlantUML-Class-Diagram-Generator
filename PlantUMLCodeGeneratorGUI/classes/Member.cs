﻿namespace PlantUMLCodeGeneratorGUI.classes
{
    public class Member
    {
        public string Type { get; internal set; }
        public string Name { get; internal set; }
        public Class OwnerClass { get; set; }
        public Method OwnerMethod { get; set; }

        private Class _type;
        private Class[] _containedTypes;

        public override string ToString()
        {
            return Name + " : " + Type;
        }

        public Class GetTypeAsClass()
        {
            return _type ?? (_type = CodeProcessor.GetTypeAsClass(Type, OwnerClass));
        }

        public Class[] GetContainedTypes()
        {
            return _containedTypes ?? (_containedTypes = CodeProcessor.GetContainedTypes(Type, OwnerClass).ToArray());
        }
    }
}