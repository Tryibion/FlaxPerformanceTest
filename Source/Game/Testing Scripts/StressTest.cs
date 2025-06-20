using FlaxEngine;
using FlaxEngine.GUI;

namespace Game
{
    /// <summary>
    /// StressTest Script.
    /// </summary>
    public class StressTest : Script
    {
        public Prefab PrefabToSpawn;
        public float DistanceApart = 200;
        public int MaxColumns = 50;
        public int MaxRows = 50;
        public int InitialNumberToSpawn = 300;
        public Actor ParentActor;

        [ReadOnly]
        public int NumberInWorld = 0;

        [EditorDisplay("UI"), ExpandGroups]
        public UIControl FPSLabelControl;

        [EditorDisplay("UI")]
        public UIControl SpawnButtonControl;

        [EditorDisplay("UI")]
        public UIControl SpawnAmountTextControl;

        [EditorDisplay("UI")]
        public UIControl TotalInstancesLabelControl;

        [EditorDisplay("UI")]
        public UIControl ResetButtonControl;

        private Button _spawnButton;
        private Label _fpsLabel;
        private TextBox _spawnAmountText;
        private Label _totalInstancesControl;
        private Button _resetButton;

        /// <inheritdoc/>
        public override void OnStart()
        {
            // Here you can add code that needs to be called when script is created, just before the first game update
            _spawnButton = SpawnButtonControl.Get<Button>();
            _resetButton = ResetButtonControl.Get<Button>();
            _fpsLabel = FPSLabelControl.Get<Label>();
            _spawnAmountText = SpawnAmountTextControl.Get<TextBox>();
            if (_spawnAmountText != null)
                _spawnAmountText.Text = InitialNumberToSpawn.ToString();
            _totalInstancesControl = TotalInstancesLabelControl.Get<Label>();

            if (_resetButton != null)
                _resetButton.Clicked += Reset;

            if (_spawnButton != null)
                _spawnButton.Clicked += Spawn;
        }

        private void Spawn()
        {
            if (!ParentActor || !PrefabToSpawn)
                return;
            
            int spawnAmount = 0;
            if (!int.TryParse(_spawnAmountText.Text, out spawnAmount))
            {
                return;
            }

            var startTime = Platform.TimeSeconds;
            int column = 0;
            int row = 0;
            int stackHeight = 0;
            for (int i = 0; i < spawnAmount; i++)
            {
                var prefab = PrefabManager.SpawnPrefab(PrefabToSpawn, ParentActor);
                prefab.LocalPosition = new Vector3(column * DistanceApart, stackHeight * DistanceApart, row * DistanceApart);
                NumberInWorld += 1;

                column += 1;
                if (column > MaxColumns)
                {
                    column = 0;
                    row += 1;
                }

                if (row > MaxRows)
                {
                    row = 0;
                    stackHeight += 1;
                }
            }
            var endTime = Platform.TimeSeconds;
            Debug.Log($"Spawned {spawnAmount} in {Mathd.CeilToInt((endTime - startTime) * 1000.0)} ms");

            _totalInstancesControl.Text = $"Total Instances: {NumberInWorld}";
        }

        private void Reset()
        {
            if (!ParentActor)
                return;

            var startTime = Platform.TimeSeconds;
            var count = ParentActor.ChildrenCount;
            ParentActor.DestroyChildren(0.1f);
            var endTime = Platform.TimeSeconds;
            Debug.Log($"Destroyed {count} in {Mathd.CeilToInt((endTime - startTime) * 1000.0)} ms");

            NumberInWorld = 0;
            _totalInstancesControl.Text = $"Total Instances: {NumberInWorld}";
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            if (_fpsLabel != null)
                _fpsLabel.Text = $"FPS: {Mathf.CeilToInt(1 / Time.DeltaTime)} (avg {Engine.FramesPerSecond})";
        }

        public override void OnDestroy()
        {
            if (_resetButton != null)
                _resetButton.Clicked -= Reset;

            if (_spawnButton != null)
                _spawnButton.Clicked -= Spawn;
            base.OnDestroy();
        }
    }
}
