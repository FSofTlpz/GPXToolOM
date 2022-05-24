using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FSofTUtils.Xamarin.Control {

   /// <summary>
   /// einfaches Control zum Festlegen einer Farbe (4 Slider)
   /// </summary>
   [DesignTimeVisible(true)]
   [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class ChooseColor : ContentView {

      /// <summary>
      /// die Farbe wurde geändert
      /// </summary>
      public event EventHandler<EventArgs> ColorChanged;

      /// <summary>
      /// aktuell eingestellte Farbe
      /// </summary>
      public Color ActualColor {
         get => Color.FromRgba(ActualColorR, ActualColorG, ActualColorB, ActualColorA);
         //set {

         //   SetValue(ActualColorProperty, value);

         //   //System.Diagnostics.Debug.WriteLine("ActualColor.set(): " + ActualColor + " -> " + value);

         //   //if (!isOnInit) {
         //   //   if (ActualColor != value) {
         //   //      //System.Diagnostics.Debug.WriteLine("ActualColor.set(): " + ActualColor + " -> " + value);
         //   //      SetValue(ActualColorProperty, value);
         //   //      //BackgroundColor = value;
         //   //      //ActualColorR = value.R;
         //   //      //ActualColorG = value.G;
         //   //      //ActualColorB = value.B;
         //   //      //ActualColorA = value.A;
         //   //      //OnColorChanged(new EventArgs());
         //   //   }
         //   //}
         //}
      }

      #region  Binding-Var BorderSize

      public static readonly BindableProperty BorderSizeProperty = BindableProperty.Create(
          nameof(BorderSize),
          typeof(double),
          typeof(ChooseColor),
          5.0);

      public double BorderSize {
         get => (double)GetValue(BorderSizeProperty);
         set => SetValue(BorderSizeProperty, value);
      }

      #endregion

      #region  Binding-Var SliderMargin

      public static readonly BindableProperty SliderMarginProperty = BindableProperty.Create(
         "SliderMargin",
         typeof(Thickness),
         typeof(ChooseColor),
         new Thickness(0, 0, 0, 0));

      public Thickness SliderMargin {
         get => (Thickness)GetValue(SliderMarginProperty);
         set => SetValue(SliderMarginProperty, value);
      }

      #endregion

      #region  Binding-Vars ActualColorR, ActualColorG, ActualColorB, ActualColorA

      public static readonly BindableProperty ActualColorRProperty = BindableProperty.Create(
          nameof(ActualColorR),
          typeof(double),
          typeof(ChooseColor),
          0.0,
          propertyChanged: changeActualColor);

      public double ActualColorR {
         get => (double)GetValue(ActualColorRProperty);
         set {
            if (ActualColorR != value) 
               SetValue(ActualColorRProperty, value);
         }
      }

      public static readonly BindableProperty ActualColorGProperty = BindableProperty.Create(
          nameof(ActualColorG),
          typeof(double),
          typeof(ChooseColor),
          0.0,
          propertyChanged: changeActualColor);

      public double ActualColorG {
         get => (double)GetValue(ActualColorGProperty);
         set {
            if (ActualColorG != value) 
               SetValue(ActualColorGProperty, value);
         }
      }

      public static readonly BindableProperty ActualColorBProperty = BindableProperty.Create(
          nameof(ActualColorB),
          typeof(double),
          typeof(ChooseColor),
          0.0,
          propertyChanged: changeActualColor);

      public double ActualColorB {
         get => (double)GetValue(ActualColorBProperty);
         set {
            if (ActualColorB != value) 
               SetValue(ActualColorBProperty, value);
         }
      }

      public static readonly BindableProperty ActualColorAProperty = BindableProperty.Create(
          nameof(ActualColorA),
          typeof(double),
          typeof(ChooseColor),
          1.0,
          propertyChanged: changeActualColor);

      public double ActualColorA {
         get => (double)GetValue(ActualColorAProperty);
         set {
            if (ActualColorA != value) 
               SetValue(ActualColorAProperty, value);
         }
      }

      static void changeActualColor(BindableObject bindable, object oldValue, object newValue) {
         (bindable as ChooseColor).changeColor();
      }

      #endregion


      public ChooseColor() {
         InitializeComponent();
      }

      void changeColor() {
         BackgroundColor = ActualColor;
         OnColorChanged(new EventArgs());
      }

      protected virtual void OnColorChanged(EventArgs e) {
         ColorChanged?.Invoke(this, e);
      }

   }
}