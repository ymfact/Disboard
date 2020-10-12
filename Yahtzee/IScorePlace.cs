namespace Yahtzee
{
    interface IScorePlace
    {
        public string Initial { get; }
        public string Name { get; }
        public string Desc { get; }
        public bool IsOpen { get; }
        public int CurrentScore { get; }
        public string CurrentScoreString { get; }
        public int CalculateScore(int[] dices);
        public void Submit(int[] dices);
    }
}
