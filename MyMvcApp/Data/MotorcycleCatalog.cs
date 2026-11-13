namespace MyMvcApp.Data;

public static class MotorcycleCatalog
{
    public static IReadOnlyList<Motorcycle> Products { get; } =
    [
        new(1, "Kawasaki Ninja 400", "Kawasaki", "Sportbike", 160_000_000, "399 cc", "45 mã lực", "6 cấp", "photo-1568772585407-9361f9bf3a87", "KAW-NIN-400"),
        new(2, "Yamaha MT-03", "Yamaha", "Nakedbike", 139_000_000, "321 cc", "42 mã lực", "6 cấp", "photo-1558981403-c5f9899a28bc", "YAM-MT-03"),
        new(3, "Honda CB500F", "Honda", "Nakedbike", 184_000_000, "471 cc", "47 mã lực", "6 cấp", "photo-1558981806-ec527fa84c39", "HON-CB-500"),
        new(4, "Ducati Monster", "Ducati", "Nakedbike", 390_000_000, "937 cc", "111 mã lực", "6 cấp", "photo-1558981403-c5f9899a28bc", "DUC-MON-937"),
        new(5, "Honda CBR650R", "Honda", "Sportbike", 254_000_000, "649 cc", "94 mã lực", "6 cấp", "photo-1558981806-ec527fa84c39", "HON-CBR-650"),
        new(6, "Kawasaki Z650", "Kawasaki", "Nakedbike", 189_000_000, "649 cc", "68 mã lực", "6 cấp", "photo-1568772585407-9361f9bf3a87", "KAW-Z-650"),
        new(7, "Honda Wave Alpha 110", "Honda", "Xe số", 18_000_000, "109.2 cc", "8.2 mã lực", "4 cấp", "photo-1558981806-ec527fa84c39", "HON-WAVE-110"),
        new(8, "Honda Vision", "Honda", "Xe tay ga", 32_000_000, "109.5 cc", "8.8 mã lực", "Vô cấp", "photo-1558981403-c5f9899a28bc", "HON-VIS-110"),
        new(9, "Honda SH160i", "Honda", "Xe tay ga", 96_000_000, "156.9 cc", "16.2 mã lực", "Vô cấp", "photo-1558981806-ec527fa84c39", "HON-SH-160"),
        new(10, "Yamaha Exciter 155 VVA", "Yamaha", "Côn tay", 55_000_000, "155 cc", "17.7 mã lực", "6 cấp", "photo-1568772585407-9361f9bf3a87", "YAM-EXC-155"),
        new(11, "Yamaha NVX 155", "Yamaha", "Xe tay ga", 55_000_000, "155.1 cc", "15.4 mã lực", "Vô cấp", "photo-1558981403-c5f9899a28bc", "YAM-NVX-155"),
        new(12, "Yamaha XSR155", "Yamaha", "Nakedbike", 77_000_000, "155 cc", "19.3 mã lực", "6 cấp", "photo-1558981806-ec527fa84c39", "YAM-XSR-155"),
        new(13, "Kawasaki Versys 650", "Kawasaki", "Adventure", 340_000_000, "649 cc", "67 mã lực", "6 cấp", "photo-1568772585407-9361f9bf3a87", "KAW-VER-650"),
        new(14, "Kawasaki Vulcan S", "Kawasaki", "Cruiser", 249_000_000, "649 cc", "61 mã lực", "6 cấp", "photo-1558981403-c5f9899a28bc", "KAW-VUL-650"),
        new(15, "Honda Winner X", "Honda", "Côn tay", 47_000_000, "149.1 cc", "15.4 mã lực", "6 cấp", "photo-1558981806-ec527fa84c39", "HON-WIN-150"),
        new(16, "Honda ADV 160", "Honda", "Adventure", 94_000_000, "156.9 cc", "16 mã lực", "Vô cấp", "photo-1568772585407-9361f9bf3a87", "HON-ADV-160"),
        new(17, "Honda Rebel 500", "Honda", "Cruiser", 181_000_000, "471 cc", "46 mã lực", "6 cấp", "photo-1558981403-c5f9899a28bc", "HON-REB-500"),
        new(18, "Suzuki Raider R150", "Suzuki", "Côn tay", 52_000_000, "147.3 cc", "18.2 mã lực", "6 cấp", "photo-1558981806-ec527fa84c39", "SUZ-RAI-150"),
        new(19, "Suzuki GSX-S150", "Suzuki", "Nakedbike", 69_000_000, "147.3 cc", "18.9 mã lực", "6 cấp", "photo-1568772585407-9361f9bf3a87", "SUZ-GSX-150"),
        new(20, "Ducati Panigale V2", "Ducati", "Sportbike", 600_000_000, "955 cc", "155 mã lực", "6 cấp", "photo-1558981403-c5f9899a28bc", "DUC-PAN-V2"),
        new(21, "Vespa Primavera 150", "Vespa", "Xe tay ga", 175_000_000, "154.8 cc", "12.9 mã lực", "Vô cấp", "photo-1558981806-ec527fa84c39", "VES-PRI-150")
    ];
}
