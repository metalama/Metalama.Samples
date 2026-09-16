[NotifyPropertyChanged]
internal partial class MovingVertex
{
    public double X { get; set; }

    public double Y { get; set; }

    public double DX { get; set; }

    public double DY { get; set; }

    public double Velocity => Math.Sqrt( (this.DX * this.DX) + (this.DY * this.DY) );

    public void ApplyTime( double time )
    {
        this.X += this.DX * time;
        this.Y += this.DY * time;
    }
}