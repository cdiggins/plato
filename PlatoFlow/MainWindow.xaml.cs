using System.Diagnostics;
using System.IO;
using System.Windows;
using Peacock;

namespace Emu;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();             
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var canvas = new SvgCanvas(1600, 800);
        GraphUserControl.Render(canvas);
        var text = canvas.ToString();
        var html = $"<html><body>{text}</body></html";
        var filePath = Path.Combine(Path.GetTempPath(), "temp.html");
        File.WriteAllText(filePath, html);
        Process.Start(filePath);
    }
}