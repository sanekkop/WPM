namespace WPM
{
    public enum Mode : byte
    {
        None, Waiting, ChoiseWork, Acceptance, AcceptedItem, TransferInicialize, Transfer,
        ChoiseInventory, Inventory, SampleInventory, SamplePut, NewInventory, NewChoiseInventory,
        SampleSet,SampleSetCorrect, ControlCollect, HarmonizationInicialize, Harmonization, HarmonizationPut, LoadingInicialization, Loading,
        ChoiseWorkShipping,ChoiseWorkSample,ChoiseWorkAddressCard, Set, SetInicialization, SetCorrect, SetComplete, ChoiseDown, Down, DownComplete,
        FreeDownComplete, NewComplectation, NewComplectationComplete, SetSelfContorl, ChoiseWorkSupply,ItemCard,
        LoaderChoise, RefillChoise, LoaderChoiseLift, Loader, RefillSet, RefillSetComplete, RefillLayout, RefillLayoutComplete, RefillSetCorrect,
		SetTransfer, ChoiseWorkAcceptance, AcceptanceCross 

    }

    public enum ActionSet : byte
    {
        ScanAdress, ScanItem, EnterCount, ScanPart, ScanBox, ScanPallete, Waiting
    }

    public enum Voice : byte
    {
        None, Good, Bad
    }

    public enum MMResult : byte
    {
        None, Positive, Negative
    }

}