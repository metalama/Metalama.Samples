public partial class MainWindow
{
    public MainWindow( MainViewModel mainViewModel )
    {
        this.InitializeComponent();

        this.DataContext = mainViewModel;
    }
}