var car = new MovingVertex { X = 5, Y = 3, DX = 0.1, DY = 0.3 };
car.PropertyChanged += ( _, args ) => Console.WriteLine( $"{args.PropertyName} has changed" );

car.ApplyTime( 1.2 );