using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FSofTUtils.Xamarin.Control {

   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class TreeViewNode : ContentView {

      static ImageSource imgSrcStdItem = ImageSource.FromResource("FSofTUtils.Xamarin.Resource.Item.png", typeof(TreeViewNode).GetTypeInfo().Assembly);
      static ImageSource imgSrcStdExpanded = ImageSource.FromResource("FSofTUtils.Xamarin.Resource.OpenGlyph.png", typeof(TreeViewNode).GetTypeInfo().Assembly);
      static ImageSource imgSrcStdCollapsed = ImageSource.FromResource("FSofTUtils.Xamarin.Resource.CollpsedGlyph.png", typeof(TreeViewNode).GetTypeInfo().Assembly);


      #region Events

      /// <summary>
      /// Der <see cref="Expanded"/>-Status hat sich geändert.
      /// </summary>
      public event EventHandler<EventArgs> OnExpandedChanged;

      /// <summary>
      /// Es wurde auf den Text getippt.
      /// </summary>
      public event EventHandler<EventArgs> OnTapped;

      /// <summary>
      /// Es wurde auf den Text doppelt getippt.
      /// </summary>
      public event EventHandler<EventArgs> OnDoubleTapped;

      /// <summary>
      /// Der Text wurde gewischt.
      /// </summary>
      public event EventHandler<EventArgs> OnSwipe;

      #endregion


      #region  Binding-Var Text

      public static readonly BindableProperty TextProperty = BindableProperty.Create(
         "Text",
         typeof(string),
         typeof(string),
         "");

      /// <summary>
      /// Text
      /// </summary>
      public string Text {
         get => GetValue(TextProperty) as string;
         set => SetValue(TextProperty, value);
      }

      #endregion

      #region  Binding-Var BackColorText

      public static readonly BindableProperty BackColorTextProperty = BindableProperty.Create(
         "BackColorText",
         typeof(Color),
         typeof(Color),
         Color.White);

      /// <summary>
      /// Hintergrundfarbe des Textes
      /// </summary>
      public Color BackColorText {
         get => (Color)GetValue(BackColorTextProperty);
         set => SetValue(BackColorTextProperty, value);
      }

      #endregion

      #region  Binding-Var ColorText

      public static readonly BindableProperty ColorTextProperty = BindableProperty.Create(
         "ColorText",
         typeof(Color),
         typeof(Color),
         Color.Black);

      /// <summary>
      /// Hintergrundfarbe des Controls
      /// </summary>
      public Color ColorText {
         get => (Color)GetValue(ColorTextProperty);
         set => SetValue(ColorTextProperty, value);
      }

      #endregion

      #region  Binding-Var BackColorNode

      public static readonly BindableProperty BackColorNodeProperty = BindableProperty.Create(
         "BackColorNode",
         typeof(Color),
         typeof(Color),
         Color.White);

      /// <summary>
      /// Hintergrundfarbe des gesamten TreeNode-Items
      /// </summary>
      public Color BackColorNode {
         get => (Color)GetValue(BackColorNodeProperty);
         set => SetValue(BackColorNodeProperty, value);
      }

      #endregion

      #region  Binding-Var ChildIndent

      public static readonly BindableProperty ChildIndentMarginProperty = BindableProperty.Create(
         "ChildIndent",
         typeof(Thickness),
         typeof(Thickness),
         new Thickness(50, 0, 0, 0));

      public Thickness ChildIndentMargin {
         get => (Thickness)GetValue(ChildIndentMarginProperty);
         set => SetValue(ChildIndentMarginProperty, value);
      }

      /// <summary>
      /// linker Einzug für den Child-Bereich
      /// </summary>
      public double ChildIndent {
         get => ChildIndentMargin.Left;
         set => SetValue(ChildIndentMarginProperty, new Thickness(value, 0, 0, 0));  // ChildIndentMargin = new Thickness(value, 0, 0, 0);
      }

      #endregion

      #region  Binding-Var FontSize

      public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
         "FontSize",
         typeof(double),
         typeof(double),
         new FontSizeConverter().ConvertFromInvariantString("Medium"));

      /// <summary>
      /// Textgröße
      /// </summary>
      public double FontSize {
         get => (double)GetValue(FontSizeProperty);
         set => SetValue(FontSizeProperty, value);
      }

      #endregion

      #region  Binding-Var ImageExpanded

      public static readonly BindableProperty ImageExpandedProperty = BindableProperty.Create(
         "ImageExpanded",
         typeof(ImageSource),
         typeof(ImageSource),
         imgSrcStdExpanded);

      /// <summary>
      /// Bild für einen expandierten <see cref="TreeViewNode"/>
      /// </summary>
      public ImageSource ImageExpanded {
         get => (ImageSource)GetValue(ImageExpandedProperty);
         set => SetValue(ImageExpandedProperty, value);
      }

      #endregion

      #region  Binding-Var ImageCollapsed

      public static readonly BindableProperty ImageCollapsedProperty = BindableProperty.Create(
         "ImageCollapsed",
         typeof(ImageSource),
         typeof(ImageSource),
         imgSrcStdCollapsed);

      /// <summary>
      /// Bild für einen nicht-expandierten <see cref="TreeViewNode"/>
      /// </summary>
      public ImageSource ImageCollapsed {
         get => (ImageSource)GetValue(ImageCollapsedProperty);
         set => SetValue(ImageCollapsedProperty, value);
      }

      #endregion

      #region  Binding-Var ImageStandard

      public static readonly BindableProperty ImageStandardProperty = BindableProperty.Create(
         "ImageStandard",
         typeof(ImageSource),
         typeof(ImageSource),
         imgSrcStdItem);

      /// <summary>
      /// Bild für einen <see cref="TreeViewNode"/> ohne Childs
      /// </summary>
      public ImageSource ImageStandard {
         get => (ImageSource)GetValue(ImageStandardProperty);
         set => SetValue(ImageStandardProperty, value);
      }

      #endregion


      StackLayout XamlChildContainer {
         get => childrenStackLayout;
      }

      /// <summary>
      /// für beliebige Zusatzdaten
      /// </summary>
      public object ExtendedData { get; set; }

      /// <summary>
      /// Anzahl der Child-Nodes
      /// </summary>
      public int ChildNodes {
         get => XamlChildContainer.Children != null ?
            XamlChildContainer.Children.Count :
            0;
      }

      /// <summary>
      /// Gibt es Child-Nodes?
      /// </summary>
      public bool HasChildNodes {
         get => ChildNodes > 0;
      }

      /// <summary>
      /// Child-Nodes sichtbar?
      /// </summary>
      public bool Expanded {
         get => XamlChildContainer.IsVisible;
         set {
            if (XamlChildContainer.IsVisible != value) {
               bool change = false;
               if (value) {
                  if (HasChildNodes)
                     change = true;
               } else
                  change = true;

               if (change) {
                  XamlChildContainer.IsVisible = value;
                  changeImage();
                  OnExpandedChanged?.Invoke(this, new EventArgs());
               }
            }
         }
      }

      /// <summary>
      /// liefert den Parent-Node
      /// </summary>
      public TreeViewNode ParentNode {
         get => Parent != null &&
                Parent is StackLayout &&
                Parent.Parent != null &&
                Parent.Parent is StackLayout &&
                Parent.Parent.Parent != null &&
                Parent.Parent.Parent is TreeViewNode ?   // StackLayout -> StackLayout -> ContentView
            Parent.Parent.Parent as TreeViewNode :
            null;
      }

      /// <summary>
      /// liefert den zugehörigen <see cref="TreeView"/>
      /// </summary>
      public TreeView TreeView {
         get {
            TreeViewNode parent = ParentNode;
            if (parent != null)
               return parent.TreeView;
            else {
               if (Parent != null &&
                   Parent is StackLayout &&
                   Parent.Parent is TreeView) {
                  return Parent.Parent as TreeView;
               }
               return null;
            }
         }
      }


      public TreeViewNode() {
         InitializeComponent();
         Expanded = false;
      }

      public TreeViewNode(string text, object data = null) : this() {
         Text = text;
         ExtendedData = data;
      }

      /// <summary>
      /// liefert alle Child-Nodes
      /// </summary>
      /// <returns></returns>
      public IList<TreeViewNode> GetChildNodes() {
         return TreeView.GetChildNodes(XamlChildContainer);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> an dieser Position wird geliefert.
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public TreeViewNode GetChildNode(int idx) {
         return TreeView.GetChildNode(XamlChildContainer, idx);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird angehängt
      /// </summary>
      /// <param name="node"></param>
      public void AddChildNode(TreeViewNode node) {
         TreeView.AddChildNode(XamlChildContainer, node);
         changeImage();
      }

      /// <summary>
      /// Ein <see cref="TreeViewNode"/> für den Text wird erzeugt und angehängt.
      /// </summary>
      /// <param name="text"></param>
      /// <param name="extdata"></param>
      public void AddChildNode(string text, object extdata = null) {
         AddChildNode(new TreeViewNode(text, extdata));
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird an der Position eingefügt.
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="node"></param>
      public void InsertChildNode(int pos, TreeViewNode node) {
         TreeView.InsertChildNode(XamlChildContainer, pos, node);
         changeImage();
      }

      /// <summary>
      /// Ein <see cref="TreeViewNode"/> für den Text wird erzeugt und eingefügt.
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="text"></param>
      /// <param name="extdata"></param>
      public void InsertChildNode(int pos, string text, object extdata = null) {
         InsertChildNode(pos, new TreeViewNode(text, extdata));
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> an der Position wird entfernt.
      /// </summary>
      /// <param name="pos"></param>
      public void RemoveChildNode(int pos) {
         TreeView.RemoveChildNode(XamlChildContainer, pos);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird entfernt.
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      public bool RemoveChildNode(TreeViewNode node) {
         return TreeView.RemoveChildNode(XamlChildContainer, node);
      }

      protected void changeImage() {
         ImageSource imageSource;
         if (Expanded) {
            imageSource = ImageExpanded;
         } else {
            if (HasChildNodes)
               imageSource = ImageCollapsed;
            else
               imageSource = ImageStandard;
         }
         if (!image.Source.Equals(imageSource))
            image.Source = imageSource;
      }

      private void ImageTapGestureRecognizer_SingleTapped(object sender, System.EventArgs e) {
         //Debug.WriteLine(">>> Image SingleTapped " + Text);
         Expanded = !Expanded;
      }

      private void FrameTapGestureRecognizer_SingleTapped(object sender, System.EventArgs e) {
         //Debug.WriteLine(">>> Frame SingleTapped " + Text);
         select();
         OnTapped?.Invoke(this, new EventArgs());
      }

      private void FrameTapGestureRecognizer_DoubleTapped(object sender, System.EventArgs e) {
         //Debug.WriteLine(">>> Frame DoubleTapped " + Text);
         select();
         OnDoubleTapped?.Invoke(this, new EventArgs());
      }

      private void FrameSwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e) {
         //Debug.WriteLine(">>> Frame Swipe " + Text);
         select();
         OnSwipe?.Invoke(this, new EventArgs());
      }

      void select() {
         TreeView tv = TreeView;
         if (tv != null)
            tv.SelectedNode = this;
      }

      public override string ToString() {
         return Text;
      }
   }
}