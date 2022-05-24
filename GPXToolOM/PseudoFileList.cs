using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace GPXToolOM {

   class PseudoFileList {

      public class Item {

         public string Data1 { get; }

         public string Data2 { get; }

         public Item(string data1, string data2) {
            Data1 = data1;
            Data2 = data2;
         }

         public override string ToString() {
            return string.Format("{0} / {1}", Data1, Data2);
         }
      }

      class ItemAndFrame {

         public Item Item { get; }

         public Frame Frame { get; }

         public ItemAndFrame(Item item, Frame frame) {
            Item = item;
            Frame = frame;
         }

      }

      public class TappedEventArgs : EventArgs {
         /// <summary>
         /// ausgewähltes Item
         /// </summary>
         public Item Item { get; private set; }

         /// <summary>
         /// akt. Pos. in der Itemliste
         /// </summary>
         public int Position { get; private set; }

         public TappedEventArgs(Item item, int position) {
            Item = item;
            Position = position;
         }

         public override string ToString() {
            return string.Format("{0}: {1}", Position, Item);
         }
      }

      TapGestureRecognizer tapGestureRecognizer4Frame;

      List<ItemAndFrame> ItemAndFrames;

      /// <summary>
      /// das StackLayout, in dem die Elemente angezeigt werden
      /// </summary>
      public StackLayout ParentStack { get; }

      Style FrameStyle;
      Style Dat1Style;
      Style Dat2Style;


      public int Count {
         get {
            return ItemAndFrames.Count;
         }
      }

      /// <summary>
      /// wird beim Tippen auf ein Element ausgelöst
      /// </summary>
      public event EventHandler<TappedEventArgs> OnFrameTapped;


      public PseudoFileList(StackLayout parentstack,
                        string framestylename,
                        string dat1stylename,
                        string dat2stylename) {
         ItemAndFrames = new List<ItemAndFrame>();
         ParentStack = parentstack;

         Element ve = ParentStack;
         while (!(ve is Page)) {
            ve = ve.Parent;
            if (ve == null)
               throw new Exception("Der ParentStack hat keine übergeordnete Page.");
            if (ve is Page) {
               Page mypage = ve as Page;

               FrameStyle = mypage.Resources.ContainsKey(framestylename) ? mypage.Resources[framestylename] as Style : Application.Current.Resources[framestylename] as Style;
               Dat1Style = mypage.Resources.ContainsKey(dat1stylename) ? mypage.Resources[dat1stylename] as Style : Application.Current.Resources[dat1stylename] as Style;
               Dat2Style = mypage.Resources.ContainsKey(dat2stylename) ? mypage.Resources[dat2stylename] as Style : Application.Current.Resources[dat2stylename] as Style;
               break;
            }
         }

         tapGestureRecognizer4Frame = new TapGestureRecognizer();
         tapGestureRecognizer4Frame.Tapped += TapGestureRecognizer4Frame_Tapped;
      }

      public void Clear() {
         ParentStack.Children.Clear();
         ItemAndFrames.Clear();
      }

      public void Insert(int pos, Item dat) {
         if (0 <= pos && pos <= ItemAndFrames.Count) {
            if (pos <= ItemAndFrames.Count)
               Add(dat);
            else {
               Frame frame = CreateCell(dat);
               ItemAndFrames.Insert(pos, new ItemAndFrame(dat, frame));
               ParentStack.Children.Insert(pos, frame);
            }
         }
      }

      public void Add(Item dat) {
         Frame frame = CreateCell(dat);
         ItemAndFrames.Add(new ItemAndFrame(dat, frame));
         ParentStack.Children.Add(frame);
      }

      public void RemoveAt(int pos) {
         if (0 <= pos && pos < ItemAndFrames.Count) {
            ItemAndFrames.RemoveAt(pos);
            ParentStack.Children.RemoveAt(pos);
         }
      }

      public Item Get(int pos) {
         if (0 <= pos && pos < ItemAndFrames.Count)
            return ItemAndFrames[pos].Item;
         return null;
      }

      /*
      <Frame Style="{StaticResource PseudolistCellFrame}">
         <StackLayout Orientation="Vertical">
            <Label Text="ghi 0123456789" TextColor="Black" />
            <Label Text="123456789 Bytes" TextColor="Black" />
         </StackLayout>
         <Frame.GestureRecognizers>
            <TapGestureRecognizer Tapped="PseudoListviewSrcFiles_Tapped"/>
         </Frame.GestureRecognizers>
      </Frame>
      */
      Frame CreateCell(Item dat) {
         StackLayout sl = new StackLayout {
            Orientation = StackOrientation.Vertical
         };

         sl.Children.Add(new Label {
            Style = Dat1Style,
            Text = dat.Data1
         });

         sl.Children.Add(new Label {
            Style = Dat2Style,
            Text = dat.Data2
         });

         Frame frame = new Frame {
            Style = FrameStyle,
            Content = sl
         };

         frame.GestureRecognizers.Add(tapGestureRecognizer4Frame);

         return frame;
      }

      void TapGestureRecognizer4Frame_Tapped(object sender, EventArgs e) {
         if (sender is Frame) {
            Frame frame = sender as Frame;
            for (int i = 0; i < ItemAndFrames.Count; i++)
               if (frame == ItemAndFrames[i].Frame) {
                  OnFrameTapped?.Invoke(sender, new TappedEventArgs(ItemAndFrames[i].Item, i));
                  break;
               }
         }
      }


   }


}
