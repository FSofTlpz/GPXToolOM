using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FSofTUtils.Xamarin.Control {
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class TreeView : ContentView {

      #region Events

      public class TreeViewNodeEventArgs : EventArgs {

         public TreeViewNode TreeViewNode { get; }

         public TreeViewNode OldTreeViewNode { get; }

         public int Position { get; }

         public int OldPosition { get; }


         public TreeViewNodeEventArgs(TreeViewNode tn, int pos = -1) :
            this(tn, null, pos) {
         }

         public TreeViewNodeEventArgs(TreeViewNode tn, TreeViewNode oldtn, int pos = -1, int oldpos = -1) {
            TreeViewNode = tn;
            Position = pos;
            OldTreeViewNode = oldtn;
            OldPosition = oldpos;
         }
      }


      /// <summary>
      /// Bei einem <see cref="TreeViewNode"/> ändert sich <see cref="TreeViewNode.Expanded"/>.
      /// </summary>
      public event EventHandler<TreeViewNodeEventArgs> OnExpandedChanged;

      /// <summary>
      /// Der ausgewählte Node hat sich geändert.
      /// </summary>
      public event EventHandler<TreeViewNodeEventArgs> OnSelectedNodeChanged;

      /// <summary>
      /// Auf einen <see cref="TreeViewNode"/> wurde getippt.
      /// </summary>
      public event EventHandler<TreeViewNodeEventArgs> OnNodeTapped;

      /// <summary>
      /// Auf einen <see cref="TreeViewNode"/> wurde doppelt getippt.
      /// </summary>
      public event EventHandler<TreeViewNodeEventArgs> OnNodeDoubleTapped;

      /// <summary>
      /// Ein <see cref="TreeViewNode"/> wurde gewischt.
      /// </summary>
      public event EventHandler<TreeViewNodeEventArgs> OnNodeSwiped;

      #endregion

      #region  Binding-Var ColorText

      public static readonly BindableProperty ColorTextProperty = BindableProperty.Create(
         "ColorText",
         typeof(Color),
         typeof(Color),
         Color.Black);

      /// <summary>
      /// Textfarbe eines TreeNode-Items
      /// </summary>
      public Color ColorText {
         get => (Color)GetValue(ColorTextProperty);
         set => SetValue(ColorTextProperty, value);
      }

      #endregion

      #region  Binding-Var ColorSelectedText

      public static readonly BindableProperty ColorSelectedTextProperty = BindableProperty.Create(
         "ColorSelectedText",
         typeof(Color),
         typeof(Color),
         Color.Black);

      /// <summary>
      /// Textfarbe eines ausgewählten TreeNode-Items
      /// </summary>
      public Color ColorSelectedText {
         get => (Color)GetValue(ColorSelectedTextProperty);
         set => SetValue(ColorSelectedTextProperty, value);
      }

      #endregion

      #region  Binding-Var BackColorSelectedNode

      public static readonly BindableProperty BackColorSelectedNodeProperty = BindableProperty.Create(
         "BackColorSelectedNode",
         typeof(Color),
         typeof(Color),
         Color.LightGray);

      /// <summary>
      /// Hintergrundfarbe eines ausgewählten TreeNode-Items
      /// </summary>
      public Color BackColorSelectedNode {
         get => (Color)GetValue(BackColorSelectedNodeProperty);
         set => SetValue(BackColorSelectedNodeProperty, value);
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


      StackLayout XamlChildContainer {
         get => treeViewStackLayout;
      }

      TreeViewNode selectedNode;
      public TreeViewNode SelectedNode {
         get => selectedNode;
         set {
            if (selectedNode == null ||
                !selectedNode.Equals(value)) {
               TreeViewNode oldSelectedNode = SelectedNode;
               if (oldSelectedNode != null) {
                  selectedNode.BackColorNode = BackgroundColor;
                  selectedNode.BackColorText = BackgroundColor;
                  selectedNode.ColorText = ColorText;
               }
               selectedNode = value;

               selectedNode.BackColorNode = BackColorSelectedNode;
               selectedNode.BackColorText = BackColorSelectedNode;
               selectedNode.ColorText = ColorSelectedText;

               OnSelectedNodeChanged?.Invoke(this, new TreeViewNodeEventArgs(selectedNode, oldSelectedNode));
            }
         }
      }


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


      public TreeView() {
         InitializeComponent();
      }


      /// <summary>
      /// liefert alle Child-Nodes im Container
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <returns></returns>
      public static IList<TreeViewNode> GetChildNodes(StackLayout xamlChildContainer) {
         List<TreeViewNode> result = new List<TreeViewNode>();
         foreach (View item in xamlChildContainer.Children) {
            if (item is TreeViewNode)
               result.Add(item as TreeViewNode);
         }
         return result;
      }

      /// <summary>
      /// liefert alle Child-Nodes
      /// </summary>
      /// <returns></returns>
      public IList<TreeViewNode> GetChildNodes() {
         return GetChildNodes(XamlChildContainer);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> an dieser Position im Container wird geliefert.
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      public static TreeViewNode GetChildNode(StackLayout xamlChildContainer, int idx) {
         return isChildIdxValid(xamlChildContainer, idx) ?
                     xamlChildContainer.Children[idx] as TreeViewNode :
                     null;
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> an dieser Position wird geliefert.
      /// </summary>
      /// <param name="idx"></param>
      /// <returns></returns>
      public TreeViewNode GetChildNode(int idx) {
         return GetChildNode(XamlChildContainer, idx);
      }

      /// <summary>
      /// liefert den Index des <see cref="TreeViewNode"/> (oder -1)
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <param name="tn"></param>
      /// <returns></returns>
      public static int GetChildIndex(StackLayout xamlChildContainer, TreeViewNode tn) {
         for (int i = 0; i < xamlChildContainer.Children.Count; i++)
            if (xamlChildContainer.Children[i] is TreeViewNode &&
                tn.Equals(xamlChildContainer.Children[i]))
               return i;
         return -1;
      }

      /// <summary>
      /// liefert den Index des <see cref="TreeViewNode"/> (oder -1)
      /// </summary>
      /// <param name="tn"></param>
      /// <returns></returns>
      public int GetChildIndex(TreeViewNode tn) {
         return GetChildIndex(XamlChildContainer, tn);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird im Container angehängt
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <param name="node"></param>
      public static void AddChildNode(StackLayout xamlChildContainer, TreeViewNode node) {
         xamlChildContainer.Children.Add(node);
         SetPropsFromTreeView(node.TreeView, node);
         node.TreeView?.registerTreeViewNodeEvents(node);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird angehängt
      /// </summary>
      /// <param name="node"></param>
      public void AddChildNode(TreeViewNode node) {
         AddChildNode(XamlChildContainer, node);
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
      /// Der <see cref="TreeViewNode"/> wird an der Position im Container eingefügt.
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <param name="pos"></param>
      /// <param name="node"></param>
      public static void InsertChildNode(StackLayout xamlChildContainer, int pos, TreeViewNode node) {
         xamlChildContainer.Children.Insert(pos, node);
         SetPropsFromTreeView(node.TreeView, node);
         node.TreeView?.registerTreeViewNodeEvents(node);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird an der Position eingefügt.
      /// </summary>
      /// <param name="pos"></param>
      /// <param name="node"></param>
      public void InsertChildNode(int pos, TreeViewNode node) {
         InsertChildNode(XamlChildContainer, pos, node);
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
      /// Der <see cref="TreeViewNode"/> an der Position im Container wird entfernt.
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <param name="pos"></param>
      public static void RemoveChildNode(StackLayout xamlChildContainer, int pos) {
         if (isChildIdxValid(xamlChildContainer, pos))
            xamlChildContainer.Children.RemoveAt(pos);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> an der Position wird entfernt.
      /// </summary>
      /// <param name="pos"></param>
      public void RemoveChildNode(int pos) {
         RemoveChildNode(XamlChildContainer, pos);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird im Container entfernt.
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <param name="node"></param>
      /// <returns></returns>
      public static bool RemoveChildNode(StackLayout xamlChildContainer, TreeViewNode node) {
         return xamlChildContainer.Children.Remove(node);
      }

      /// <summary>
      /// Der <see cref="TreeViewNode"/> wird entfernt.
      /// </summary>
      /// <param name="node"></param>
      /// <returns></returns>
      public bool RemoveChildNode(TreeViewNode node) {
         return RemoveChildNode(XamlChildContainer, node);
      }

      /// <summary>
      /// Ist idx ein gültiger Index?
      /// </summary>
      /// <param name="xamlChildContainer"></param>
      /// <param name="idx"></param>
      /// <returns></returns>
      static bool isChildIdxValid(StackLayout xamlChildContainer, int idx) {
         return xamlChildContainer != null &&
                xamlChildContainer.Children != null &&
                0 <= idx &&
                idx < xamlChildContainer.Children.Count;
      }

      public static void SetPropsFromTreeView(TreeView tv, TreeViewNode node) {
         if (tv != null) {
            node.BackColorNode = tv.BackgroundColor;
            node.BackColorText = tv.BackgroundColor;
            node.ColorText = tv.ColorText;
         }
      }

      void registerTreeViewNodeEvents(TreeViewNode node) {
         node.OnTapped += Node_OnTapped;
         node.OnDoubleTapped += Node_OnDoubleTapped;
         node.OnSwipe += Node_OnSwipe;
         node.OnExpandedChanged += Node_OnExpandedChanged;
      }

      private void Node_OnExpandedChanged(object sender, EventArgs e) {
         OnExpandedChanged?.Invoke(this, new TreeViewNodeEventArgs(sender as TreeViewNode));
      }

      private void Node_OnSwipe(object sender, EventArgs e) {
         OnNodeSwiped?.Invoke(this, new TreeViewNodeEventArgs(sender as TreeViewNode));
      }

      private void Node_OnDoubleTapped(object sender, EventArgs e) {
         OnNodeDoubleTapped?.Invoke(this, new TreeViewNodeEventArgs(sender as TreeViewNode));
      }

      private void Node_OnTapped(object sender, EventArgs e) {
         OnNodeTapped?.Invoke(this, new TreeViewNodeEventArgs(sender as TreeViewNode));
      }


      /// <summary>
      /// einige Propertie-Änderungen für alle <see cref="TreeViewNode"/> übernehmen
      /// </summary>
      /// <param name="propertyName"></param>
      protected override void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         base.OnPropertyChanged(propertyName);

         if (propertyName == "BackgroundColor" ||
             propertyName == "ColorText" ||
             propertyName == "FontSize" ||
             propertyName == "ChildIndent") {
            changeProperty4Childs(propertyName, GetChildNodes());
         }
      }

      /// <summary>
      /// ein Propertie vom <see cref="TreeView"/> auf die Liste der <see cref="TreeViewNode"/> (und ihre Childs) übernehmen
      /// </summary>
      /// <param name="propertyName"></param>
      /// <param name="nodes"></param>
      void changeProperty4Childs(string propertyName, IList<TreeViewNode> nodes) {
         foreach (TreeViewNode node in nodes) {
            if (propertyName == "BackgroundColor") {

               node.BackgroundColor =
               node.BackColorNode =
               node.BackColorText = BackgroundColor;

            } else if (propertyName == "ColorText") {

               node.ColorText = ColorText;

            } else if (propertyName == "ChildIndent") {

               node.ChildIndent = ChildIndent;

            } else if (propertyName == "FontSize") {

               node.FontSize = FontSize;

            }

            changeProperty4Childs(propertyName, node.GetChildNodes());
         }
      }

   }
}