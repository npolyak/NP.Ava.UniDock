using NP.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace NP.TestCollectionSerialization
{
    public class Base
    {
        [XmlAttribute]
        public string? Str { get; set; }

        public Base()
        {

        }

        public Base(string? str)
        {
            Str = str;
        }
    }

    public class C1 : Base
    {
        [XmlAttribute]
        public string? Str1 { get; set; }

        public C1()
        {

        }

        public C1(string str, string str1) : base(str)
        {
            Str1 = str1;
        }
    }

    public class C2 : Base
    {
        [XmlAttribute]
        public string? Str2 { get; set; }

        public C2()
        {

        }

        public C2(string str, string str2) : base(str)
        {
            Str2 = str2;
        }
    }


    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            ObservableCollection<Base> list = new ObservableCollection<Base>();

            list.Add(new C1("Hi", "World"));
            list.Add(new C2("Hello", "World"));

            string str = 
                XmlSerializationUtils.Serialize<ObservableCollection<Base>>(list, typeof(C1), typeof(C2));


            ObservableCollection<Base> l2 = 
                XmlSerializationUtils.Deserialize<ObservableCollection<Base>>(str, false, typeof(C1), typeof(C2));
        }
    }
}
