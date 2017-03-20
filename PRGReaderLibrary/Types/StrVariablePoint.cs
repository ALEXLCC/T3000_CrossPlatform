namespace PRGReaderLibrary
{
    /// <summary>
    /// Size: 30 + 4 + 2 = 36
    /// </summary>
    public class StrVariablePoint : BasePoint
    {
        public float Value { get; set; }         /*  (4 bytes; float)*/
        public bool IsManual { get; set; } /*  (1 bit; 0=auto, 1=manual)*/
        public bool IsAnalog { get; set; }  /*  (1 bit; 0=digital, 1=analog)*/
        public bool IsControl { get; set; }
        public byte Unused { get; set; } //5
        public byte Range { get; set; } //8; /*  (1 Byte ; variable_range_equate)*/
    }
}