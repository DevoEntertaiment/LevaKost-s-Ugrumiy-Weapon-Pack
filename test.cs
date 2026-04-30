using System;
using System.Reflection;
using Terraria;

class Program {
    static void Main() {
        var type = typeof(Mount);
        foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
            Console.WriteLine("Field: " + f.FieldType.Name + " " + f.Name);
        }
        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
            Console.WriteLine("Prop: " + p.PropertyType.Name + " " + p.Name);
        }
        Console.WriteLine("--- MountData ---");
        var mtype = typeof(Mount.MountData);
        foreach (var f in mtype.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
            Console.WriteLine("Field: " + f.FieldType.Name + " " + f.Name);
        }
    }
}
