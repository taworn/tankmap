using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace tankmap {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private string title = null;
        private Image imageSource = null;

        private Map mapObject = null;
        private bool mapChanged = false;
        private string mapFileName = null;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            RoutedCommand commandFileNew = new RoutedCommand();
            commandFileNew.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandFileNew, menuFileNew_Click));

            RoutedCommand commandFileOpen = new RoutedCommand();
            commandFileOpen.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandFileOpen, menuFileOpen_Click));

            RoutedCommand commandFileSave = new RoutedCommand();
            commandFileSave.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandFileSave, menuFileSave_Click));

            RoutedCommand commandFileClose = new RoutedCommand();
            commandFileClose.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandFileClose, menuFileClose_Click));

            RoutedCommand commandFileExit = new RoutedCommand();
            commandFileExit.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(commandFileExit, menuFileExit_Click));

            title = Title;
            radioBlock.IsChecked = true;
            imageSource = imagePass;
            EnableMenu(false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!CheckFileSave()) {
                e.Cancel = true;
            }
        }

        private void menuFileNew_Click(object sender, RoutedEventArgs e) {
            if (!CheckFileSave())
                return;

            var dialog = new SetSizeWindow();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true) {
                // starts new map
                mapObject = new Map(dialog.GetResultWidth(), dialog.GetResultHeight());
                mapChanged = false;
                mapFileName = null;
                mapGrid.Columns = mapObject.GetWidth();
                mapGrid.Rows = mapObject.GetHeight();
                mapGrid.Width = mapGrid.Columns * 48;
                mapGrid.Height = mapGrid.Rows * 48;
                var children = mapGrid.Children;
                children.Clear();
                var size = mapObject.GetWidth() * mapObject.GetHeight();
                for (var i = 0; i < size; i++) {
                    var image = new Image();
                    image.Source = imagePass.Source;
                    image.Margin = new Thickness(1, 1, 1, 1);
                    image.MouseDown += Image_MouseDown;
                    image.Tag = i;
                    children.Add(image);
                }

                textSize.Text = mapObject.GetWidth() + " x " + mapObject.GetHeight();
                UpdateTitle();
                EnableMenu(true);
            }
        }

        private void menuFileOpen_Click(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.DefaultExt = ".map";
            dialog.FileName = "";
            dialog.Filter = "Map Files (*.map)|*.map|All Files (*.*)|*.*";
            var result = dialog.ShowDialog();
            if (result == true) {
                if (!CheckFileSave())
                    return;

                // opens old map
                mapObject = new Map(1, 1);
                try {
                    mapObject.Open(dialog.FileName);
                    mapChanged = false;
                    mapFileName = dialog.FileName;
                    mapGrid.Columns = mapObject.GetWidth();
                    mapGrid.Rows = mapObject.GetHeight();
                    mapGrid.Width = mapGrid.Columns * 48;
                    mapGrid.Height = mapGrid.Rows * 48;
                    var children = mapGrid.Children;
                    children.Clear();
                    var size = mapObject.GetWidth() * mapObject.GetHeight();
                    for (var i = 0; i < size; i++) {
                        var image = new Image();
                        image.Source = MapDataToImage(mapObject.Get(i)).Source;
                        image.Margin = new Thickness(1, 1, 1, 1);
                        image.MouseDown += Image_MouseDown;
                        image.Tag = i;
                        children.Add(image);
                    }

                    textSize.Text = mapObject.GetWidth() + " x " + mapObject.GetHeight();
                    UpdateTitle();
                    EnableMenu(true);
                }
                catch (IOException ex) {
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                    mapObject = null;
                    mapChanged = false;
                    mapFileName = null;
                    var children = mapGrid.Children;
                    children.Clear();
                    textSize.Text = "";
                    Title = title;
                    EnableMenu(false);
                }
            }
        }

        private void menuFileSave_Click(object sender, RoutedEventArgs e) {
            if (mapFileName != null) {
                mapObject.Save(mapFileName);
                mapChanged = false;
                UpdateTitle();
            }
            else {
                menuFileSaveAs_Click(sender, e);
            }
        }

        private void menuFileSaveAs_Click(object sender, RoutedEventArgs e) {
            var dialog = new SaveFileDialog();
            dialog.CheckPathExists = true;
            dialog.DefaultExt = ".map";
            dialog.FileName = "";
            dialog.Filter = "Map Files (*.map)|*.map|All Files (*.*)|*.*";
            var result = dialog.ShowDialog();
            if (result == true) {
                mapFileName = dialog.FileName;
                mapObject.Save(mapFileName);
                mapChanged = false;
                UpdateTitle();
            }
        }

        private void menuFileClose_Click(object sender, RoutedEventArgs e) {
            if (!CheckFileSave())
                return;

            mapObject = null;
            mapChanged = false;
            mapFileName = null;
            var children = mapGrid.Children;
            children.Clear();

            textSize.Text = "";
            Title = title;
            EnableMenu(false);
        }

        private void menuFileExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void menuHelpAbout_Click(object sender, RoutedEventArgs e) {
            var dialog = new AboutWindow();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void menuEditPass_Click(object sender, RoutedEventArgs e) {
            SurroundWith(Map.BLOCK_PASS, imagePass);
        }

        private void menuEditTree_Click(object sender, RoutedEventArgs e) {
            SurroundWith(Map.BLOCK_TREE, imageTree);
        }

        private void menuEditBrick_Click(object sender, RoutedEventArgs e) {
            SurroundWith(Map.BLOCK_BRICK, imageBrick);
        }

        private void menuEditSteel_Click(object sender, RoutedEventArgs e) {
            SurroundWith(Map.BLOCK_STEEL, imageSteel);
        }

        private void menuEditWater_Click(object sender, RoutedEventArgs e) {
            SurroundWith(Map.BLOCK_WATER, imageWater);
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var image = (Image)sender;
            if (imageSource != null) {
                int imageLocationTag = int.Parse(image.Tag.ToString());
                int imageSourceTag = int.Parse(imageSource.Tag.ToString());

                if (mapObject.Get(imageLocationTag) != imageSourceTag) {
                    if (imageSourceTag == Map.BLOCK_HERO) {
                        // removes old position of hero
                        var size = mapObject.GetWidth() * mapObject.GetHeight();
                        var i = 0;
                        while (i < size) {
                            var block = mapObject.Get(i);
                            if (block == Map.BLOCK_HERO) {
                                mapObject.Set(i, Map.BLOCK_PASS);
                                Image im = (Image)mapGrid.Children[i];
                                im.Source = imagePass.Source;
                            }
                            i++;
                        }
                    }

                    // sets new image
                    mapObject.Set(imageLocationTag, imageSourceTag);
                    image.Source = imageSource.Source;
                }

                if (!mapChanged) {
                    mapChanged = true;
                    UpdateTitle();
                }
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e) {
            var radio = (RadioButton)sender;
            if (mapObject != null) {
                var tag = radio.Tag.ToString();
                if (tag == Map.BLOCK_PASS.ToString())
                    imageSource = imagePass;
                else if (tag == Map.BLOCK_TREE.ToString())
                    imageSource = imageTree;
                else if (tag == Map.BLOCK_BRICK.ToString())
                    imageSource = imageBrick;
                else if (tag == Map.BLOCK_STEEL.ToString())
                    imageSource = imageSteel;
                else if (tag == Map.BLOCK_WATER.ToString())
                    imageSource = imageWater;
                else if (tag == Map.BLOCK_EAGLE.ToString())
                    imageSource = imageEagle;
                else if (tag == Map.BLOCK_HERO.ToString())
                    imageSource = imageHero;

                else if (tag == Map.ENEMY_00.ToString())
                    imageSource = imageTank00;
                else if (tag == Map.ENEMY_01.ToString())
                    imageSource = imageTank01;
                else if (tag == Map.ENEMY_02.ToString())
                    imageSource = imageTank02;
                else if (tag == Map.ENEMY_03.ToString())
                    imageSource = imageTank03;
                else if (tag == Map.ENEMY_04.ToString())
                    imageSource = imageTank04;
                else if (tag == Map.ENEMY_05.ToString())
                    imageSource = imageTank05;
                else if (tag == Map.ENEMY_06.ToString())
                    imageSource = imageTank06;
                else if (tag == Map.ENEMY_07.ToString())
                    imageSource = imageTank07;

                else if (tag == Map.ENEMY_10.ToString())
                    imageSource = imageTank10;
                else if (tag == Map.ENEMY_11.ToString())
                    imageSource = imageTank11;
                else if (tag == Map.ENEMY_12.ToString())
                    imageSource = imageTank12;
                else if (tag == Map.ENEMY_13.ToString())
                    imageSource = imageTank13;
                else if (tag == Map.ENEMY_14.ToString())
                    imageSource = imageTank14;
                else if (tag == Map.ENEMY_15.ToString())
                    imageSource = imageTank15;
                else if (tag == Map.ENEMY_16.ToString())
                    imageSource = imageTank16;
                else if (tag == Map.ENEMY_17.ToString())
                    imageSource = imageTank17;

                else if (tag == Map.ENEMY_20.ToString())
                    imageSource = imageTank20;
                else if (tag == Map.ENEMY_21.ToString())
                    imageSource = imageTank21;
                else if (tag == Map.ENEMY_22.ToString())
                    imageSource = imageTank22;
                else if (tag == Map.ENEMY_23.ToString())
                    imageSource = imageTank23;
                else if (tag == Map.ENEMY_24.ToString())
                    imageSource = imageTank24;
                else if (tag == Map.ENEMY_25.ToString())
                    imageSource = imageTank25;
                else if (tag == Map.ENEMY_26.ToString())
                    imageSource = imageTank26;
                else if (tag == Map.ENEMY_27.ToString())
                    imageSource = imageTank27;

                else if (tag == Map.ENEMY_30.ToString())
                    imageSource = imageTank30;
                else if (tag == Map.ENEMY_31.ToString())
                    imageSource = imageTank31;
                else if (tag == Map.ENEMY_32.ToString())
                    imageSource = imageTank32;
                else if (tag == Map.ENEMY_33.ToString())
                    imageSource = imageTank33;
                else if (tag == Map.ENEMY_34.ToString())
                    imageSource = imageTank34;
                else if (tag == Map.ENEMY_35.ToString())
                    imageSource = imageTank35;
                else if (tag == Map.ENEMY_36.ToString())
                    imageSource = imageTank36;
                else if (tag == Map.ENEMY_37.ToString())
                    imageSource = imageTank37;

                else
                    imageSource = imagePass;
            }
        }

        private Image MapDataToImage(int data) {
            switch (data) {
                default:
                case Map.BLOCK_PASS:
                    return imagePass;
                case Map.BLOCK_TREE:
                    return imageTree;
                case Map.BLOCK_BRICK:
                    return imageBrick;
                case Map.BLOCK_STEEL:
                    return imageSteel;
                case Map.BLOCK_WATER:
                    return imageWater;
                case Map.BLOCK_EAGLE:
                    return imageEagle;
                case Map.BLOCK_HERO:
                    return imageHero;

                case Map.ENEMY_00:
                    return imageTank00;
                case Map.ENEMY_01:
                    return imageTank01;
                case Map.ENEMY_02:
                    return imageTank02;
                case Map.ENEMY_03:
                    return imageTank03;
                case Map.ENEMY_04:
                    return imageTank04;
                case Map.ENEMY_05:
                    return imageTank05;
                case Map.ENEMY_06:
                    return imageTank06;
                case Map.ENEMY_07:
                    return imageTank07;

                case Map.ENEMY_10:
                    return imageTank10;
                case Map.ENEMY_11:
                    return imageTank11;
                case Map.ENEMY_12:
                    return imageTank12;
                case Map.ENEMY_13:
                    return imageTank13;
                case Map.ENEMY_14:
                    return imageTank14;
                case Map.ENEMY_15:
                    return imageTank15;
                case Map.ENEMY_16:
                    return imageTank16;
                case Map.ENEMY_17:
                    return imageTank17;

                case Map.ENEMY_20:
                    return imageTank20;
                case Map.ENEMY_21:
                    return imageTank21;
                case Map.ENEMY_22:
                    return imageTank22;
                case Map.ENEMY_23:
                    return imageTank23;
                case Map.ENEMY_24:
                    return imageTank24;
                case Map.ENEMY_25:
                    return imageTank25;
                case Map.ENEMY_26:
                    return imageTank26;
                case Map.ENEMY_27:
                    return imageTank27;

                case Map.ENEMY_30:
                    return imageTank30;
                case Map.ENEMY_31:
                    return imageTank31;
                case Map.ENEMY_32:
                    return imageTank32;
                case Map.ENEMY_33:
                    return imageTank33;
                case Map.ENEMY_34:
                    return imageTank34;
                case Map.ENEMY_35:
                    return imageTank35;
                case Map.ENEMY_36:
                    return imageTank36;
                case Map.ENEMY_37:
                    return imageTank37;
            }
        }

        private bool CheckFileSave() {
            if (mapObject != null && mapChanged) {
                if (MessageBox.Show(this, "Map data does not save.  Are you sure you want to discard?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.No)
                    return false;
            }
            return true;
        }

        private void EnableMenu(bool enabled) {
            menuFileSave.IsEnabled = enabled;
            menuFileSaveAs.IsEnabled = enabled;
            menuFileClose.IsEnabled = enabled;
            menuEditPass.IsEnabled = enabled;
            menuEditTree.IsEnabled = enabled;
            menuEditBrick.IsEnabled = enabled;
            menuEditSteel.IsEnabled = enabled;
            menuEditWater.IsEnabled = enabled;
        }

        private void SurroundWith(int blockInt, Image blockImage) {
            var children = mapGrid.Children;

            var start = 0;
            for (var i = start; i < start + mapObject.GetWidth(); i++) {
                mapObject.Set(i, blockInt);
                ((Image)children[i]).Source = blockImage.Source;
            }

            start = (mapObject.GetHeight() - 1) * mapObject.GetWidth();
            for (var i = start; i < start + mapObject.GetWidth(); i++) {
                mapObject.Set(i, blockInt);
                ((Image)children[i]).Source = blockImage.Source;
            }

            start = mapObject.GetWidth();
            while (start < (mapObject.GetHeight() - 1) * mapObject.GetWidth()) {
                mapObject.Set(start, blockInt);
                ((Image)children[start]).Source = blockImage.Source;
                start += mapObject.GetWidth();
            }

            start = mapObject.GetWidth() * 2 - 1;
            while (start < (mapObject.GetHeight() - 1) * mapObject.GetWidth()) {
                mapObject.Set(start, blockInt);
                ((Image)children[start]).Source = blockImage.Source;
                start += mapObject.GetWidth();
            }
        }

        private void UpdateTitle() {
            Title = (mapChanged ? "* " : "") + (mapFileName != null ? mapFileName : "new map") + " - " + title;
        }

    }
}
