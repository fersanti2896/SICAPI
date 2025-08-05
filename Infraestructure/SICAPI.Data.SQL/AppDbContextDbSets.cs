using Microsoft.EntityFrameworkCore;
using SICAPI.Data.SQL.Entities;

namespace SICAPI.Data.SQL;

public partial class AppDbContext
{
    public virtual DbSet<TLog> TLogs { get; set; }
    public virtual DbSet<TUsers> TUsers { get; set; }
    public virtual DbSet<TRol> TRol { get; set; }
    public virtual DbSet<TRefreshTokens> TRefreshTokens { get; set; }
    public virtual DbSet<TSuppliers> TSuppliers { get; set; }
    public virtual DbSet<TProducts> TProducts { get; set; }
    public virtual DbSet<TProductProviders> TProductProviders { get; set; }
    public virtual DbSet<TEntradasAlmacen> TEntradasAlmacen { get; set; }
    public virtual DbSet<TEntradaDetalle> TEntradaDetalle { get; set; }
    public virtual DbSet<TInventory> TInventory { get; set; }
    public virtual DbSet<TClients> TClients { get; set; }
    public virtual DbSet<TPostalCodes> TPostalCodes { get; set; }
    public virtual DbSet<TClientsAddress> TClientsAddress { get; set; }
    public virtual DbSet<TSales> TSales { get; set; }
    public virtual DbSet<TSalesDetail> TSalesDetail { get; set; }
    public virtual DbSet<TSaleStatuses> TSaleStatuses { get; set; }
    public virtual DbSet<TPaymentStatuses> TPaymentStatuses { get; set; }
    public virtual DbSet<TPayments> TPayments { get; set; }
    public virtual DbSet<TCancelledSalesComments> TCancelledSalesComments { get; set; }
}