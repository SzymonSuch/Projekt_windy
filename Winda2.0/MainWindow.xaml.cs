using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Winda2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int wysokosc_windy = 60;
        private int Floor = 0;

        private HashSet<int> pendingFloors = new();
        private bool isMoving = false;
        private int Direction = 0; // 1 = góra, -1 = dół
        private bool delayScheduled = false;

        public MainWindow()
        {
            InitializeComponent();
            Canvas.SetTop(Winda, (10 - Floor) * wysokosc_windy);
            Canvas.SetLeft(Winda, 40);
        }

        public async Task GoToFloor(int targetFloor)
        {
            if (targetFloor != Floor)
            {
                pendingFloors.Add(targetFloor); // zapamiętaj każde piętro
            }

            if (!isMoving && !delayScheduled)
            {
                delayScheduled = true;
                await Task.Delay(1500); // zawsze czekaj 500ms po pierwszym kliknięciu
                delayScheduled = false;

                if (pendingFloors.Count > 0)
                {
                    Direction = pendingFloors.First() > Floor ? 1 : -1;
                    await ProcessQueue();
                }
            }
        }
        private async void Flor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Content.ToString(), out int targetFloor))
            {
                await GoToFloor(targetFloor);
            }
        }
        private async Task ProcessQueue()
        {
            isMoving = true;

            while (pendingFloors.Count > 0)
            {
                List<int> targets;
                // Sortuj piętra w zależności od kierunku
                if (Direction == 1)
                    targets = pendingFloors.Where(f => f > Floor).OrderBy(f => f).ToList();
                else
                    targets = pendingFloors.Where(f => f < Floor).OrderByDescending(f => f).ToList();

                if (targets.Count == 0)
                {
                    // Sprawdź, czy są jeszcze piętra w przeciwnym kierunku
                    Direction *= -1;
                    continue;
                }

                foreach (int nextFloor in targets)
                {
                    pendingFloors.Remove(nextFloor);
                    await MoveWindaTo(nextFloor);
                }
            }

            isMoving = false;
            Direction = 0;
        }
        private async Task MoveWindaTo(int floor)
        {
            try { 
            double targetY = (10 - floor) * wysokosc_windy;

            await CloseDoors();

            DoubleAnimation animation = new()
            {
                To = targetY,
                Duration = TimeSpan.FromSeconds(1),
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.3
            };

            Storyboard.SetTarget(animation, Winda);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));

            Storyboard sb = new();
            sb.Children.Add(animation);

            TaskCompletionSource<bool> tcs = new();
            sb.Completed += (s, e) => tcs.SetResult(true);
            sb.Begin();

            await Task.Delay(500);
            await tcs.Task;
            Floor = floor;
            await OpenDoors();

                // TU POJAWIA SIĘ OKIENKO PYTAJĄCE
                if (floor == 1)
                {
                    var result = MessageBox.Show("Czy wysiadasz na 1 piętrze?", "Winda", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        pietro1 pietro1 = new pietro1(this);
                        pietro1.Show();
                    }
                }

                await Task.Delay(2000);
            await CloseDoors();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task OpenDoors()
        {
            DoubleAnimation left = new() { To = -40, Duration = TimeSpan.FromSeconds(0.5) };
            DoubleAnimation right = new() { To = 40, Duration = TimeSpan.FromSeconds(0.5) };

            Ldrzwi.RenderTransform = new TranslateTransform();
            Pdrzwi.RenderTransform = new TranslateTransform();

            if (Ldrzwi.RenderTransform == null || !(Ldrzwi.RenderTransform is TranslateTransform))
                Ldrzwi.RenderTransform = new TranslateTransform();
            if (Pdrzwi.RenderTransform == null || !(Pdrzwi.RenderTransform is TranslateTransform))
                Pdrzwi.RenderTransform = new TranslateTransform();

            (Ldrzwi.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.XProperty, left);
            (Pdrzwi.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.XProperty, right);

            await Task.Delay(500); // czekaj aż drzwi się otworzą

            //await CloseDoors();
        }

        private async Task CloseDoors()
        {
            DoubleAnimation left = new() { To = 0, Duration = TimeSpan.FromSeconds(0.5) };
            DoubleAnimation right = new() { To = 0, Duration = TimeSpan.FromSeconds(0.5) };

            if (Ldrzwi.RenderTransform == null || !(Ldrzwi.RenderTransform is TranslateTransform))
                Ldrzwi.RenderTransform = new TranslateTransform();
            if (Pdrzwi.RenderTransform == null || !(Pdrzwi.RenderTransform is TranslateTransform))
                Pdrzwi.RenderTransform = new TranslateTransform();

            (Ldrzwi.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.XProperty, left);
            (Pdrzwi.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.XProperty, right);

            await Task.Delay(500); // czekaj aż drzwi się zamkną
        }

    }

}