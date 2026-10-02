internal partial class MovingVertex : Vertex
{
    public double DX { get; set; }

    public double EndX => this.X + this.DX;

    public double DY { get; set; }

    public double EndY => this.Y + this.DY;

    public double Velocity => Math.Sqrt( (this.DX * this.DX) + (this.DY * this.DY) );

    public void ApplyTime( double time )
    {
        this.X += this.DX * time;
        this.Y += this.DY * time;
    }
}