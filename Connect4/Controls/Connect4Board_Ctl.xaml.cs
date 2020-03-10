<UserControl x:Class="Connect4.Connect4Board_Ctl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             xmlns:local="clr-namespace:Connect4_TestApplication"
             mc:Ignorable="d" 
             d:DesignHeight="450" d:DesignWidth="450">
    <UserControl.Resources>
        <DataTemplate x:Key="Connect4Row_DataTemplate">
            <ItemsControl ItemsSource="{Binding RowData}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <StackPanel Orientation="Horizontal"/>
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Grid Background="Blue" Width="50" Height="50">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition/>
                                <ColumnDefinition Width="8*"/>
                                <ColumnDefinition/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition/>
                                <RowDefinition Height="8*"/>
                                <RowDefinition/>
                            </Grid.RowDefinitions>
                            <Ellipse Grid.Row="1" Grid.Column="1" HorizontalAlignment="Stretch" VerticalAlignment="Stretch">
                                <Ellipse.Fill>
                                    <Binding Path="CheckerState">
                                        <Binding.Converter>
                                            <local:CheckerEnumToColorConverter/>
                                        </Binding.Converter>
                                    </Binding>
                                </Ellipse.Fill>
                            </Ellipse>
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </DataTemplate>
    </UserControl.Resources>
    <Grid>
        <ItemsControl ItemsSource="{Binding BoardData}" ItemTemplate="{StaticResource Connect4Row_DataTemplate}"/>
    </Grid>
</UserControl>
