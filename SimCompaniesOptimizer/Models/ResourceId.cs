﻿namespace SimCompaniesOptimizer.Models;

public static class NotSellableResourceIds
{
    public static readonly HashSet<ResourceId> NotSellableResources = new()
    {
        ResourceId.SubOrbitalRocket,
        ResourceId.SubOrbital2ndStage,
        ResourceId.OrbitalBooster,
        ResourceId.Starship,
        ResourceId.BFR,
        ResourceId.JumboJet,
        ResourceId.LuxuryJet,
        ResourceId.SingleEnginePlane,
        ResourceId.Satellite,
        ResourceId.AerospaceResearch
    };
}

public enum ResourceId
{
    Power = 1,
    Water,
    Apples,
    Oranges,
    Grapes,
    Grain,
    Steak,
    Sausages,
    Eggs,
    CrudeOil,
    Petrol,
    Diesel,
    Transport,
    Minerals,
    Bauxite,
    Silicon,
    Chemicals,
    Aluminium,
    Plastic,
    Processors,
    ElectronicComponents,
    Batteries,
    Displays,
    SmartPhones,
    Tablets,
    Laptops,
    Monitors,
    Televisions,
    PlantResearch,
    EnergyResearch,
    MiningResearch,
    ElectronicsResearch,
    BreedingResearch,
    ChemistryResearch,
    Software,
    Cotton = 40,
    Fabric,
    IronOre,
    Steel,
    Sand,
    Glass,
    Leather,
    OnBoardComputer,
    ElectricMotor,
    LuxuryCarInterior,
    BasicInterior,
    CarBody,
    CombustionEngine,
    EconomyECar,
    LuxuryECar,
    EconomyCar,
    LuxuryCar,
    Truck,
    AutomotiveResearch,
    FashionResearch,
    Underwear,
    Gloves,
    Dress,
    StilettoHeel,
    Handbags,
    Sneakers,
    Seeds,
    XmasCrackers,
    GoldOre,
    GoldenBars,
    LuxuryWatch,
    Necklace,
    Sugarcane,
    Ethanol,
    Methane,
    CarbonFibers,
    CarbonComposite,
    Fuselage,
    Wing,
    HighGradeEComps,
    FlightComputer,
    Cockpit,
    AttitudeControl,
    RocketFuel,
    PropellantTank,
    SolidFuelBooster,
    RocketEngine,
    HeatShield,
    IonDrive,
    JetEngine,
    SubOrbital2ndStage,
    SubOrbitalRocket,
    OrbitalBooster,
    Starship,
    BFR,
    JumboJet,
    LuxuryJet,
    SingleEnginePlane,
    Quadcopter,
    Satellite,
    AerospaceResearch,
    ReinforcedConcrete,
    Bricks,
    Cement,
    Clay,
    Limestone,
    Wood,
    SteelBeams,
    Planks,
    Windows,
    Tools,
    ConstructionUnits,
    Bulldozer,
    MaterialsResearch,
    Robots,
    Cows,
    Pigs,
    Milk,
    CoffeeBeans,
    CoffeePowder,
    Vegetables,
    Bread,
    Cheese,
    ApplePie,
    OrangeJuice,
    AppleCider,
    GingerBeer,
    FrozenPizza,
    Pasta,
    Hamburger,
    Lasagna,
    MeatBalls,
    Cocktails,
    Flour,
    Butter,
    Sugar,
    Cocoa,
    Dough,
    Sauce,
    Fodder,
    Chocolate,
    VegetableOil,
    Salad,
    Samosa,
    Recipes = 145
}