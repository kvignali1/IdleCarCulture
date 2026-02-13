namespace IdleCarCulture
{
    /// <summary>
    /// Central configuration for all game balancing constants.
    /// Modify these values to tune game difficulty, economy, and progression.
    /// </summary>
    public static class Tuning
    {
        // ==================== HEAT SYSTEM ====================

        /// <summary>
        /// Heat threshold for low heat tier (triggers police at ~1%).
        /// </summary>
        public const float HEAT_THRESHOLD_LOW = 30f;

        /// <summary>
        /// Heat threshold for medium heat tier (triggers police at ~5%).
        /// </summary>
        public const float HEAT_THRESHOLD_MEDIUM = 60f;

        /// <summary>
        /// Heat threshold for high heat tier (triggers police at ~15%).
        /// </summary>
        public const float HEAT_THRESHOLD_HIGH = 85f;

        /// <summary>
        /// Police encounter trigger chance at low heat (per update frame).
        /// </summary>
        public const float HEAT_TRIGGER_CHANCE_LOW = 0.01f;

        /// <summary>
        /// Police encounter trigger chance at medium heat (per update frame).
        /// </summary>
        public const float HEAT_TRIGGER_CHANCE_MEDIUM = 0.05f;

        /// <summary>
        /// Police encounter trigger chance at high heat (per update frame).
        /// </summary>
        public const float HEAT_TRIGGER_CHANCE_HIGH = 0.15f;

        /// <summary>
        /// Heat lost when successfully escaping police (as percentage of current heat).
        /// </summary>
        public const float POLICE_ESCAPE_HEAT_REDUCTION_PERCENT = 0.5f;

        /// <summary>
        /// Heat lost when caught by police (as percentage of current heat, typically all).
        /// </summary>
        public const float POLICE_CAUGHT_HEAT_LOSS_PERCENT = 1f;

        /// <summary>
        /// Base fine amount when caught by police (before heat scaling).
        /// </summary>
        public const long POLICE_BASE_FINE = 500;

        /// <summary>
        /// Fine multiplier per heat point (e.g., 50 heat = 50 * POLICE_FINE_HEAT_MULTIPLIER added).
        /// </summary>
        public const float POLICE_FINE_HEAT_MULTIPLIER = 50f;

        /// <summary>
        /// Police escape chance base (before grip/suspension bonus).
        /// </summary>
        public const float POLICE_ESCAPE_CHANCE_BASE = 0.3f;

        /// <summary>
        /// Grip+Suspension multiplier for police escape chance per point.
        /// </summary>
        public const float POLICE_ESCAPE_STAT_BONUS = 0.001f;

        // ==================== ECONOMY ====================

        /// <summary>
        /// Base payout per tier for illegal races.
        /// Index 0 = Tier 1, Index 4 = Tier 5.
        /// </summary>
        public static readonly int[] ILLEGAL_BASE_PAYOUT = { 500, 1000, 1500, 2500, 4000 };

        /// <summary>
        /// Base payout per tier for legal races.
        /// Index 0 = Tier 1, Index 4 = Tier 5.
        /// </summary>
        public static readonly int[] LEGAL_BASE_PAYOUT = { 1000, 2000, 3500, 5500, 8000 };

        /// <summary>
        /// Prestige bonus to race payout (as percentage per prestige level).
        /// E.g., 2% per prestige = 1.02x at prestige 1, 1.04x at prestige 2.
        /// </summary>
        public const float PRESTIGE_PAYOUT_BONUS_PERCENT = 0.02f;

        // ==================== PRESTIGE ====================

        /// <summary>
        /// Money required to qualify for prestige.
        /// </summary>
        public const long PRESTIGE_MONEY_THRESHOLD = 500000;

        /// <summary>
        /// Reputation required to qualify for prestige.
        /// </summary>
        public const int PRESTIGE_REPUTATION_THRESHOLD = 1000;

        /// <summary>
        /// Cred required to qualify for prestige.
        /// </summary>
        public const int PRESTIGE_CRED_THRESHOLD = 500;

        /// <summary>
        /// Starting money balance after prestige.
        /// </summary>
        public const long PRESTIGE_STARTING_MONEY = 5000;

        /// <summary>
        /// Cost reduction percentage per prestige level (e.g., 5% per prestige).
        /// </summary>
        public const float PRESTIGE_COST_REDUCTION_PERCENT = 0.05f;

        /// <summary>
        /// Heat gain reduction percentage per prestige level (e.g., 3% per prestige).
        /// </summary>
        public const float PRESTIGE_HEAT_REDUCTION_PERCENT = 0.03f;

        /// <summary>
        /// Income bonus percentage per prestige level (e.g., 10% per prestige).
        /// </summary>
        public const float PRESTIGE_INCOME_BONUS_PERCENT = 0.10f;

        // ==================== UPGRADES ====================

        /// <summary>
        /// Exponential multiplier applied per upgrade level for cost scaling.
        /// Cost = baseCost * (costExponent ^ level).
        /// </summary>
        public const float UPGRADE_COST_EXPONENT = 1.8f;

        /// <summary>
        /// Maximum level cap for any upgrade.
        /// </summary>
        public const int UPGRADE_MAX_LEVEL = 5;

        // ==================== RACE MECHANICS ====================

        /// <summary>
        /// PR multiplier for illegal races (emphasizes power).
        /// </summary>
        public const float RACE_PR_ILLEGAL_MULTIPLIER = 1.1f;

        /// <summary>
        /// PR multiplier for legal races (slightly reduces raw power emphasis).
        /// </summary>
        public const float RACE_PR_LEGAL_MULTIPLIER = 0.95f;

        /// <summary>
        /// Skill multiplier contribution for illegal races (affects player PR).
        /// </summary>
        public const float RACE_SKILL_ILLEGAL_MULTIPLIER = 0.10f;

        /// <summary>
        /// Skill multiplier contribution for legal races (affects player PR).
        /// </summary>
        public const float RACE_SKILL_LEGAL_MULTIPLIER = 0.15f;

        /// <summary>
        /// PR randomness range lower bound (e.g., 0.97 = -3%).
        /// </summary>
        public const float RACE_RANDOMNESS_MIN = 0.97f;

        /// <summary>
        /// PR randomness range upper bound (e.g., 1.03 = +3%).
        /// </summary>
        public const float RACE_RANDOMNESS_MAX = 1.03f;

        /// <summary>
        /// Win payout multiplier scaling (1.5 / prRatio), clamped to this range.
        /// </summary>
        public const float RACE_PAYOUT_WIN_MULTIPLIER_MIN = 0.5f;

        /// <summary>
        /// Win payout multiplier scaling (1.5 / prRatio), clamped to this range.
        /// </summary>
        public const float RACE_PAYOUT_WIN_MULTIPLIER_MAX = 3.0f;

        /// <summary>
        /// Baseline win payout multiplier (before difficulty scaling).
        /// </summary>
        public const float RACE_PAYOUT_WIN_BASELINE = 1.5f;

        /// <summary>
        /// Loss payout multiplier (participation reward).
        /// </summary>
        public const float RACE_PAYOUT_LOSS_MULTIPLIER = 0.1f;

        /// <summary>
        /// Opponent stat scale per tier (e.g., 1.1x per tier).
        /// </summary>
        public const float OPPONENT_STAT_SCALE_PER_TIER = 1.1f;

        // ==================== MINIGAME ====================

        /// <summary>
        /// Duration of tire heat phase in seconds.
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_DURATION = 3f;

        /// <summary>
        /// Start of green zone for tire heat (0..1 range).
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_GREEN_START = 0.4f;

        /// <summary>
        /// End of green zone for tire heat (0..1 range).
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_GREEN_END = 0.6f;

        /// <summary>
        /// Tire heat optimal holding window start (seconds).
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_OPTIMAL_START = 1.2f;

        /// <summary>
        /// Tire heat optimal holding window end (seconds).
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_OPTIMAL_END = 1.8f;

        /// <summary>
        /// Tire heat good holding window start (seconds).
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_GOOD_START = 0.9f;

        /// <summary>
        /// Tire heat good holding window end (seconds).
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_GOOD_END = 2.1f;

        /// <summary>
        /// Score when in optimal tire heat zone.
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_OPTIMAL_SCORE = 1f;

        /// <summary>
        /// Score when in good tire heat zone.
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_GOOD_SCORE = 0.7f;

        /// <summary>
        /// Score when outside tire heat zones.
        /// </summary>
        public const float MINIGAME_TIRE_HEAT_BAD_SCORE = 0.3f;

        /// <summary>
        /// Duration of launch phase in seconds.
        /// </summary>
        public const float MINIGAME_LAUNCH_DURATION = 3f;

        /// <summary>
        /// Min random target time for launch tap (seconds into phase).
        /// </summary>
        public const float MINIGAME_LAUNCH_TARGET_MIN = 0.8f;

        /// <summary>
        /// Max random target time for launch tap (seconds into phase).
        /// </summary>
        public const float MINIGAME_LAUNCH_TARGET_MAX = 2f;

        /// <summary>
        /// Perfect launch tap timing tolerance (seconds).
        /// </summary>
        public const float MINIGAME_LAUNCH_PERFECT_TOLERANCE = 0.1f;

        /// <summary>
        /// Good launch tap timing tolerance (seconds).
        /// </summary>
        public const float MINIGAME_LAUNCH_GOOD_TOLERANCE = 0.3f;

        /// <summary>
        /// OK launch tap timing tolerance (seconds).
        /// </summary>
        public const float MINIGAME_LAUNCH_OK_TOLERANCE = 0.5f;

        /// <summary>
        /// Perfect launch score.
        /// </summary>
        public const float MINIGAME_LAUNCH_PERFECT_SCORE = 1f;

        /// <summary>
        /// Good launch score.
        /// </summary>
        public const float MINIGAME_LAUNCH_GOOD_SCORE = 0.8f;

        /// <summary>
        /// OK launch score.
        /// </summary>
        public const float MINIGAME_LAUNCH_OK_SCORE = 0.5f;

        /// <summary>
        /// Bad launch score.
        /// </summary>
        public const float MINIGAME_LAUNCH_BAD_SCORE = 0.2f;

        /// <summary>
        /// Duration of shift phase in seconds per shift.
        /// </summary>
        public const float MINIGAME_SHIFT_DURATION = 2f;

        /// <summary>
        /// Number of shifts in the shifting phase.
        /// </summary>
        public const int MINIGAME_SHIFT_COUNT = 3;

        /// <summary>
        /// Min random marker position for shift (0..1 range).
        /// </summary>
        public const float MINIGAME_SHIFT_MARKER_MIN = 0.3f;

        /// <summary>
        /// Max random marker position for shift (0..1 range).
        /// </summary>
        public const float MINIGAME_SHIFT_MARKER_MAX = 0.7f;

        /// <summary>
        /// Perfect shift tap tolerance (distance from marker).
        /// </summary>
        public const float MINIGAME_SHIFT_PERFECT_TOLERANCE = 0.1f;

        /// <summary>
        /// Good shift tap tolerance (distance from marker).
        /// </summary>
        public const float MINIGAME_SHIFT_GOOD_TOLERANCE = 0.2f;

        /// <summary>
        /// OK shift tap tolerance (distance from marker).
        /// </summary>
        public const float MINIGAME_SHIFT_OK_TOLERANCE = 0.4f;

        /// <summary>
        /// Perfect shift score.
        /// </summary>
        public const float MINIGAME_SHIFT_PERFECT_SCORE = 1f;

        /// <summary>
        /// Good shift score.
        /// </summary>
        public const float MINIGAME_SHIFT_GOOD_SCORE = 0.8f;

        /// <summary>
        /// OK shift score.
        /// </summary>
        public const float MINIGAME_SHIFT_OK_SCORE = 0.5f;

        /// <summary>
        /// Bad shift score.
        /// </summary>
        public const float MINIGAME_SHIFT_BAD_SCORE = 0.2f;

        /// <summary>
        /// Delay after each shift phase (seconds).
        /// </summary>
        public const float MINIGAME_SHIFT_DELAY = 0.5f;

        /// <summary>
        /// Time scale for smooth score lerp during minigame.
        /// </summary>
        public const float MINIGAME_SCORE_LERP_SPEED = 2f;

        // ==================== SKILL WEIGHTING ====================

        /// <summary>
        /// Launch phase weight in overall skill calculation.
        /// </summary>
        public const float SKILL_WEIGHT_LAUNCH = 0.4f;

        /// <summary>
        /// Shift phase weight in overall skill calculation.
        /// </summary>
        public const float SKILL_WEIGHT_SHIFT = 0.4f;

        /// <summary>
        /// Tire heat phase weight in overall skill calculation.
        /// </summary>
        public const float SKILL_WEIGHT_TIRE_HEAT = 0.2f;

        // ==================== RACE OPPORTUNITY SPAWNING ====================

        /// <summary>
        /// Cred required per tier unlock for illegal races.
        /// E.g., 100 cred = Tier 1, 200 cred = Tier 2, etc.
        /// </summary>
        public const int OPPORTUNITY_CRED_PER_TIER = 100;

        /// <summary>
        /// Reputation required per tier unlock for legal races.
        /// E.g., 150 rep = Tier 1, 300 rep = Tier 2, etc.
        /// </summary>
        public const int OPPORTUNITY_REPUTATION_PER_TIER = 150;

        /// <summary>
        /// Min bet suggestion as percentage of player money.
        /// </summary>
        public const float OPPORTUNITY_BET_MIN_PERCENT = 0.05f;

        /// <summary>
        /// Max bet suggestion as percentage of player money.
        /// </summary>
        public const float OPPORTUNITY_BET_MAX_PERCENT = 0.10f;

        /// <summary>
        /// Max bet suggestion cap (can't exceed this as percentage of money).
        /// </summary>
        public const float OPPORTUNITY_BET_MAX_MONEY_FRACTION = 0.5f;

        /// <summary>
        /// Base bet per tier for opportunity generation.
        /// </summary>
        public const long OPPORTUNITY_BASE_BET = 500;
    }
}
