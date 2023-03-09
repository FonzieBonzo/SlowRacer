using System.Net.Http.Json;
using System.Threading.Tasks;
using SlowRacer.Components;

namespace SlowRacer
{
    public partial class Index
    {
        int _index = 0;
        int[][] _data;

        protected override async Task OnInitializedAsync()
        {
            _data = await Http.GetFromJsonAsync<int[][]>("Data/Track1.json");
            Update();
        }

        async void Update()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(100);
                    MoveCar();
                }
                catch { }
            }
        }

        void MoveCar()
        {
            _index += 10;
            if (_index >= _data.Length)
            {
                _index = 0;
            }

            Car.Cars[0].Left = _data[_index][0] - 10;
            Car.Cars[0].Top = _data[_index][1] - 10;
            Car.Cars[0].Update();
        }
    }
}
