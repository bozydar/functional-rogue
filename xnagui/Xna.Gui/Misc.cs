namespace Xna.Gui {

    public enum Direction { None, Vertical, Horizontal, Both }    

    public enum TextHorizontal { LeftAligned, CenterAligned, RightAligned }

    public enum TextVertical { TopAligned, CenterAligned, BottomAligned }

    public delegate void ElementEvent(Widget sender);
}
