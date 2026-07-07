namespace PlatoWinFormsEditor;

public class Experiment
{
    public int MyField = 3;
    public int MyProp => 4;
    public int MyMethod() => 5;
}

public static class MyStaticClass
{
    public static int MyField(this Experiment self) => self.MyField;
    public static int MyProp(this Experiment self) => self.MyProp;
    public static int MyMethod(this Experiment self) => self.MyMethod();
}

