public static class StoryProgress
{
    public static bool BasementOpeningDialoguePlayed { get; private set; }
    public static bool HolyChestOpened { get; private set; }
    public static bool KaelHasArmorAndSword { get; private set; }

    public static void MarkBasementOpeningDialoguePlayed()
    {
        BasementOpeningDialoguePlayed = true;
    }

    public static void MarkHolyChestOpened()
    {
        HolyChestOpened = true;
        KaelHasArmorAndSword = true;
    }
}