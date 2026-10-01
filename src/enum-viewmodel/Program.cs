using ViewModels;

var viewModel = new VisibilityViewModel( Visibility.Collapsed );
Console.WriteLine( $"IsCollapsed={viewModel.IsCollapsed}" );