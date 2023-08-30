//using System.Collections.Generic;
//using WebApp_StockFixx.Models;

//namespace WebApp_StockFixx.Data
//    {
//        public static class DummyData
//        {
//            private static List<WarehouseItem> dummyData = new List<WarehouseItem>
//            {
//                 new WarehouseItem
//                {
//                    SerialNumber = "RIFW062300006",
//                    TagNo = "00006",
//                    StorageBin = "-",
//                    Material = "0.64X0.64 1/2H",
//                    MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                    ActualQty = 13300,
//                    Unit = "G",
//                    StorageType = "P001",
//                    VendorCode = "001",
//                    VendorName = "PT PAL",
//                    IsVMI = true
//                },
//                new WarehouseItem
//                {
//                    SerialNumber = "DUMMY002",
//                    TagNo = "DUMMY-1002",
//                    StorageBin = "Bin-2",
//                    Material = "Material-2",
//                    MaterialDescription = "Description-2",
//                    ActualQty = 20,
//                    Unit = "Unit-2",
//                    StorageType = "-",
//                    VendorCode = "-",
//                    VendorName = "-",
//                    IsVMI = false
//                },
//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300001",
//                TagNo = "00001",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/2H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 10000,
//                Unit = "G",
//                StorageType = "P001",
//                VendorCode = "001",
//                VendorName = "PT PAL",
//                IsVMI = true
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300002",
//                TagNo = "00002",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/4H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 15000,
//                Unit = "G",
//                StorageType = "-",
//                VendorCode = "-",
//                VendorName = "-",
//                IsVMI = false
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300003",
//                TagNo = "00003",
//                StorageBin = "-",
//                Material = "0.64X0.64 3/4H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 20000,
//                Unit = "G",
//                StorageType = "P002",
//                VendorCode = "002",
//                VendorName = "PT XYZ",
//                IsVMI = true
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300004",
//                TagNo = "00004",
//                StorageBin = "-",
//                Material = "0.64X0.64 1H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 8000,
//                Unit = "G",
//                StorageType = "-",
//                VendorCode = "-",
//                VendorName = "-",
//                IsVMI = false
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300005",
//                TagNo = "00005",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/8H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 5000,
//                Unit = "G",
//                StorageType = "P003",
//                VendorCode = "003",
//                VendorName = "PT ABC",
//                IsVMI = true
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300006",
//                TagNo = "00006",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/16H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 12000,
//                Unit = "G",
//                StorageType = "-",
//                VendorCode = "-",
//                VendorName = "-",
//                IsVMI = false
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300007",
//                TagNo = "00007",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/32H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 7000,
//                Unit = "G",
//                StorageType = "P004",
//                VendorCode = "004",
//                VendorName = "PT DEF",
//                IsVMI = true
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300008",
//                TagNo = "00008",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/64H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 18000,
//                Unit = "G",
//                StorageType = "-",
//                VendorCode = "-",
//                VendorName = "-",
//                IsVMI = false
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300009",
//                TagNo = "00009",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/128H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 24000,
//                Unit = "G",
//                StorageType = "P005",
//                VendorCode = "005",
//                VendorName = "PT GHI",
//                IsVMI = true
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300010",
//                TagNo = "00010",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/256H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 32000,
//                Unit = "G",
//                StorageType = "-",
//                VendorCode = "-",
//                VendorName = "-",
//                IsVMI = false
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300011",
//                TagNo = "00011",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/512H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 6000,
//                Unit = "G",
//                StorageType = "P006",
//                VendorCode = "006",
//                VendorName = "PT JKL",
//                IsVMI = true
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300012",
//                TagNo = "00012",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/1024H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 4000,
//                Unit = "G",
//                StorageType = "-",
//                VendorCode = "-",
//                VendorName = "-",
//                IsVMI = false
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300013",
//                TagNo = "00013",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/2048H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 27000,
//                Unit = "G",
//                StorageType = "P007",
//                VendorCode = "007",
//                VendorName = "PT MNO",
//                IsVMI = true
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300014",
//                TagNo = "00014",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/4096H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 9000,
//                Unit = "G",
//                StorageType = "-",
//                VendorCode = "-",
//                VendorName = "-",
//                IsVMI = false
//                },

//                new WarehouseItem
//                {
//                SerialNumber = "RIFW062300015",
//                TagNo = "00015",
//                StorageBin = "-",
//                Material = "0.64X0.64 1/8192H",
//                MaterialDescription = "TIN PLANED SQUARE CP WIRE",
//                ActualQty = 15000,
//                Unit = "G",
//                StorageType = "P008",
//                VendorCode = "008",
//                VendorName = "PT PQR",
//                IsVMI = true
//                },
//            };

//            // Fungsi untuk mendapatkan data dummy
//            public static List<WarehouseItem> GetDummyData()
//            {
//                return dummyData;
//            }

//            // Fungsi untuk memperbarui data dummy dengan data yang baru diupload
//            public static void UpdateDummyData(List<WarehouseItem> newData)
//            {
//                dummyData.Clear(); // Hapus data dummy yang lama
//                dummyData.AddRange(newData); // Tambahkan data baru dari upload
//            }
//        }
//    }
