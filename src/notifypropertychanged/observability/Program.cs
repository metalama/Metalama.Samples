var vertex = new MovingVertex { X = 5, Y = 3, DX = 0.1, DY = 0.3 };
vertex.PropertyChanged += ( _, args ) => Console.WriteLine( $"{args.PropertyName} has changed" );

vertex.ApplyTime( 1.2 );

// Changing DX also notifies a change of Velocity, which depends on it.
vertex.DX = 0.5;
