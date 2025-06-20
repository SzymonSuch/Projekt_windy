using System.Collections.ObjectModel;
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
        public ObservableCollection<RideEntry> RideHistory { get; set; } = new(); // Właściwość do przechowywania historii jazd

        private HashSet<int> pendingFloors = new();
        public bool isMoving = false;
        private int Direction = 0; // 1 = góra, -1 = dół
        private bool delayScheduled = false;
        private Window? activePietroWindow = null;
        public pietro1? pietro1Window;
        public pietro2? pietro2Window;
        private bool doorsOpen = false;
        private CancellationTokenSource? moveDelayCts;
        private bool waitingToMove = false;
        private Dictionary<int, Window> pietroWindows = new();


        
        public MainWindow() // konstruktor
        {
            InitializeComponent();

            this.DataContext = this; // Ustawienie DataContext dla powiązań danych

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
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int targetFloor))
            {
                // Jeśli jesteś w windzie i winda jest na piętrze
                if (isUserInElevator && targetFloor == Floor && !isMoving)
                {
                    if (!doorsOpen)
                    {
                        await OpenDoors();
                    }

                    var result = MessageBox.Show(
                        $"Czy chcesz wyjść na piętro {targetFloor}?",
                        "Winda",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Przed utworzeniem nowego okna piętra
                        if (pietroWindows.ContainsKey(targetFloor) && pietroWindows[targetFloor].IsVisible)
                        {
                            // Okno już istnieje i jest otwarte, nie twórz nowego
                            return;
                        }

                        var window = CreatePietroWindow(targetFloor);
                        if (window != null)
                        {
                            window.Show();
                            isUserInElevator = false;
                            await CloseDoors();
                        }
                    }
                    else
                    {
                        await CloseDoors();
                    }
                }
                // Jeśli NIE jesteś w windzie, a winda jest na piętrze i nie jedzie
                else if (!isUserInElevator && targetFloor == Floor && !isMoving)
                {
                    if (!doorsOpen)
                    {
                        await OpenDoors();
                    }

                    var result = MessageBox.Show(
                        $"Czy chcesz wejść do windy na piętrze {targetFloor}?",
                        "Winda",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        isUserInElevator = true;
                        // Zamknij okno piętra, jeśli jest otwarte
                        if (pietroWindows.ContainsKey(targetFloor))
                        {
                            pietroWindows[targetFloor].Close();
                            pietroWindows.Remove(targetFloor);
                        }
                        await CloseDoors();
                    }
                    else
                    {
                        await CloseDoors();
                    }
                }
                else
                {
                    // Standardowa obsługa jazdy windy
                    await GoToFloor(targetFloor);
                }
            }
        }
        private async Task ProcessQueue()
        {
            isMoving = true;

            while (pendingFloors.Count > 0)
            {
                List<int> targets;

                if (Direction == 1)
                    targets = pendingFloors.Where(f => f > Floor).OrderBy(f => f).ToList();
                else
                    targets = pendingFloors.Where(f => f < Floor).OrderByDescending(f => f).ToList();

                if (targets.Count == 0)
                {
                    Direction *= -1;
                    continue;
                }

                foreach (int nextFloor in targets)
                {
                    // Nie usuwamy teraz z pendingFloors
                    var result = await MoveWindaTo(nextFloor);

                    if (result)
                    {
                        pendingFloors.Remove(nextFloor); // usuwamy tylko jeśli udało się dojechać
                    }
                    else
                    {
                        // np. drzwi otwarte – nie kontynuujemy, czekamy na nową próbę
                        break;
                    }
                }
            }

            isMoving = false;
            Direction = 0;
        }

        public int CurrentDirection => Direction;

        private bool isUserInElevator = true;

        private async Task<bool> MoveWindaTo(int floor)
        {
        
            try { 
            double targetY = (10 - floor) * wysokosc_windy;

                //await CloseDoors();
                if (doorsOpen)
                {
                    await CloseDoors();

                    waitingToMove = true;
                    moveDelayCts = new CancellationTokenSource();

                    try
                    {
                        await Task.Delay(2000, moveDelayCts.Token); // czekaj 2s na ewentualne "OpenDrzwi"
                    }
                    catch (TaskCanceledException)
                    {
                        waitingToMove = false;
                        return false; // nie udało się ruszyć
                    }

                    waitingToMove = false;
                }


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
             
            int previousFloor = Floor; ;
            Floor = floor;

                RideHistory.Add(new RideEntry(previousFloor, floor)); // Dodaj wpis do historii jazd

                if (pietro1Window != null && pietro1Window.IsVisible)
            {
                pietro1Window.UpdateFloorDisplay(Floor);
            }
            if (pietro2Window != null && pietro2Window.IsVisible)
            {
                pietro2Window.UpdateFloorDisplay(Floor);
            }
            await OpenDoors();


                // Obsługa wszystkich pięter (przykład dla pięter 0-10)
                if (floor >= 0 && floor <= 10)
                {
                    // Najpierw obsługa wysiadania, jeśli pasażer jest w windzie
                    if (isUserInElevator)
                    {
                        var result = MessageBox.Show(
                            $"Czy wysiadasz na piętrze {floor}?",
                            "Winda",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            isUserInElevator = false;
                            // Tworzenie nowego okna piętra tylko jeśli nie istnieje
                            if (!pietroWindows.ContainsKey(floor) || !pietroWindows[floor].IsVisible)
                            {
                                var window = CreatePietroWindow(floor);
                                if (window != null)
                                {
                                    pietroWindows[floor] = window;
                                    activePietroWindow = window;
                                    window.Closed += (s, e) => { if (pietroWindows.ContainsKey(floor)) pietroWindows.Remove(floor); activePietroWindow = null; };
                                    (window as dynamic).UpdateFloorDisplay(Floor);
                                    window.Show();
                                }
                            }
                        }
                    }
                    // Następnie obsługa wchodzenia, jeśli okno piętra jest otwarte
                    else if (pietroWindows.ContainsKey(floor) && pietroWindows[floor].IsVisible)
                    {
                        var result = MessageBox.Show(
                            $"Czy chcesz wejść do windy na piętrze {floor}?",
                            "Winda",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            isUserInElevator = true;
                            pietroWindows[floor].Close();
                            pietroWindows.Remove(floor);
                            activePietroWindow = null;
                        }
                    }
                }


            await Task.Delay(2000);
            //await CloseDoors();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return true;
        }
        public int CurrentFloor => Floor;
        public bool IsPendingFloorsEmpty => !pendingFloors.Any();
        public async Task OpenDoors()
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

            doorsOpen = true;
            await Task.Delay(500);
            doorsOpen = true;
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

            doorsOpen = false;
            await Task.Delay(500);
            doorsOpen = false;
        }

        private async void OpenDrzwi_Click(object sender, RoutedEventArgs e)
        {
            // Nie pozwalaj otworzyć drzwi tylko podczas faktycznego ruchu windy
            if (isMoving && !waitingToMove)
            {
                MessageBox.Show("Nie można otworzyć drzwi podczas jazdy!", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Jeśli trwa oczekiwanie na ruszenie, anuluj je i natychmiast otwórz drzwi
            if (waitingToMove && moveDelayCts != null)
            {
                moveDelayCts.Cancel();
                waitingToMove = false;
                moveDelayCts = null;
                await OpenDoors();
                return;
            }

            // Sprawdź stan drzwi
            bool drzwiZamkniete = true;

            if (Ldrzwi.RenderTransform is TranslateTransform lTrans && lTrans.X != 0)
                drzwiZamkniete = false;
            if (Pdrzwi.RenderTransform is TranslateTransform pTrans && pTrans.X != 0)
                drzwiZamkniete = false;

            // Jeżeli drzwi już są otwarte — nie rób nic
            if (!drzwiZamkniete)
                return;

            // Otwórz drzwi
            await OpenDoors();

            // Po otwarciu – poczekaj i kontynuuj jazdę, jeśli trzeba
            if (!isMoving && pendingFloors.Count > 0)
            {
                await Task.Delay(2000);
                await CloseDoors();
                await ProcessQueue();
            }
        }

        private async void CloseDrzwi_Click(object sender, RoutedEventArgs e)
        {
            await CloseDoors();
        }

        private Window? CreatePietroWindow(int floor)
        {
            // Dodaj kolejne case'y jeśli masz osobne klasy dla każdego piętra
            switch (floor)
            {
                case 0: return new pietro0(this);
                case 1: return new pietro1(this);
                case 2: return new pietro2(this);
                case 3: return new pietro3(this);
                case 4: return new pietro4(this);
                case 5: return new pietro5(this);
                case 6: return new pietro6(this);
                case 7: return new pietro7(this);
                case 8: return new pietro8(this);
                case 9: return new pietro9(this);
                case 10: return new pietro10(this);
                case -1: return new pietro_1(this);
                default: return null;
            }
        }
        public bool AreDoorsOpen => doorsOpen;

        public void SetUserInElevator(bool value)
        {
            isUserInElevator = value;
        }

        private void Dzwon_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Dryń dryń dryń....", "Dzwoń:", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}

//      zrobione - jak otwierasz drzwi i jesteś na pietrze to niech się pojawia okienko czy chesz wejść do windy
// czujnik pietra sie zepsół, pokazuje tylko pietro na którym jesteś, nie śledzi windy
//      zrobione - dodać resztę pięter.
// NIE WIEM KURWA JAK , naprawić przycisk OpenDrzwi.(jak jedzie winda to dzwi mogą się otworzyć)
//      zrobione - Zrobić żeby aplikacja zaczynała się od okna pietro0, a nie mainWindow
// dodać pietro -1 i tylko za podaniem login i hasło można tam pojechać i wyjść.
// grafika do windy, drzwi, przyciski, piętra, winda, tło