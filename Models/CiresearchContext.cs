using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace CIResearch.Models;

public partial class CiresearchContext : DbContext
{
    public CiresearchContext()
    {
    }

    public CiresearchContext(DbContextOptions<CiresearchContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdministrativeRegion> AdministrativeRegions { get; set; }

    public virtual DbSet<AdministrativeUnit> AdministrativeUnits { get; set; }

    public virtual DbSet<Cir1556Ncl> Cir1556Ncls { get; set; }

    public virtual DbSet<Cir1569Crystal> Cir1569Crystals { get; set; }

    public virtual DbSet<Cir15771578Colina2> Cir15771578Colina2s { get; set; }

    public virtual DbSet<Cir16023x> Cir16023xes { get; set; }

    public virtual DbSet<Cir1607Clm> Cir1607Clms { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Efmigrationshistory> Efmigrationshistories { get; set; }

    public virtual DbSet<F1Apollo> F1Apollos { get; set; }

    public virtual DbSet<HeaderConfig> HeaderConfigs { get; set; }

    public virtual DbSet<Iqvia1charlie> Iqvia1charlies { get; set; }

    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<Marriagestatus> Marriagestatuses { get; set; }

    public virtual DbSet<Navy> Navies { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<Respondent> Respondents { get; set; }

    public virtual DbSet<Typeproject> Typeprojects { get; set; }

    public virtual DbSet<Ward> Wards { get; set; }

    public virtual DbSet<WelltrackHcp> WelltrackHcps { get; set; }

    public virtual DbSet<WelltrackPharmacist> WelltrackPharmacists { get; set; }

    public virtual DbSet<_01Bigbang> _01Bigbangs { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdministrativeRegion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("administrative_regions");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name");
            entity.Property(e => e.CodeNameEn)
                .HasMaxLength(255)
                .HasColumnName("code_name_en");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn)
                .HasMaxLength(255)
                .HasColumnName("name_en");
        });

        modelBuilder.Entity<AdministrativeUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("administrative_units");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name");
            entity.Property(e => e.CodeNameEn)
                .HasMaxLength(255)
                .HasColumnName("code_name_en");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(255)
                .HasColumnName("full_name_en");
            entity.Property(e => e.ShortName)
                .HasMaxLength(255)
                .HasColumnName("short_name");
            entity.Property(e => e.ShortNameEn)
                .HasMaxLength(255)
                .HasColumnName("short_name_en");
        });

        modelBuilder.Entity<Cir1556Ncl>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("cir1556_ncl");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<Cir1569Crystal>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("cir1569_crystal");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<Cir15771578Colina2>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("cir1577_1578_colina2");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<Cir16023x>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("cir_1602_3x");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<Cir1607Clm>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("cir_1607_clm");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("districts");

            entity.HasIndex(e => e.ProvinceCode, "idx_districts_province");

            entity.HasIndex(e => e.AdministrativeUnitId, "idx_districts_unit");

            entity.HasIndex(e => e.NameEn, "name_en").HasAnnotation("MySql:FullTextIndex", true);

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(255)
                .HasColumnName("full_name_en");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn).HasColumnName("name_en");
            entity.Property(e => e.ProvinceCode)
                .HasMaxLength(20)
                .HasColumnName("province_code");

            entity.HasOne(d => d.AdministrativeUnit).WithMany(p => p.Districts)
                .HasForeignKey(d => d.AdministrativeUnitId)
                .HasConstraintName("districts_administrative_unit_id_fkey");

            entity.HasOne(d => d.ProvinceCodeNavigation).WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceCode)
                .HasConstraintName("districts_province_code_fkey");
        });

        modelBuilder.Entity<Efmigrationshistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId).HasName("PRIMARY");

            entity.ToTable("__efmigrationshistory");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<F1Apollo>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("f1_apollo");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<HeaderConfig>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("header_config");

            entity.Property(e => e.SourceHeader)
                .HasMaxLength(50)
                .HasColumnName("source_header")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TargetHeader)
                .HasMaxLength(50)
                .HasColumnName("target_header")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Iqvia1charlie>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("iqvia_1charlie");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("link");

            entity.Property(e => e.IsProcess).HasColumnName("is_process");
            entity.Property(e => e.Url)
                .HasMaxLength(1000)
                .HasColumnName("url")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Marriagestatus>(entity =>
        {
            entity.HasKey(e => e.MarriageStatus1).HasName("PRIMARY");

            entity.ToTable("marriagestatus");

            entity.Property(e => e.MarriageStatus1)
                .HasMaxLength(100)
                .HasColumnName("marriage_status")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Navy>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("navy");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectName).HasName("PRIMARY");

            entity.ToTable("projects");

            entity.HasIndex(e => e.NameType, "fk_project_type");

            entity.Property(e => e.ProjectName)
                .HasMaxLength(100)
                .HasColumnName("project_name");
            entity.Property(e => e.NameType)
                .HasMaxLength(100)
                .HasColumnName("name_type")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ObjectOfUse)
                .HasMaxLength(100)
                .HasColumnName("object_of_use")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ProjectId)
                .HasMaxLength(20)
                .HasColumnName("project_id");
            entity.Property(e => e.ProjectYear)
                .HasColumnType("year")
                .HasColumnName("project_year");

            entity.HasOne(d => d.NameTypeNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.NameType)
                .HasConstraintName("fk_project_type");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("provinces");

            entity.HasIndex(e => e.AdministrativeRegionId, "idx_provinces_region");

            entity.HasIndex(e => e.AdministrativeUnitId, "idx_provinces_unit");

            entity.HasIndex(e => e.NameEn, "name_en").HasAnnotation("MySql:FullTextIndex", true);

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.AdministrativeRegionId).HasColumnName("administrative_region_id");
            entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(255)
                .HasColumnName("full_name_en");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn).HasColumnName("name_en");

            entity.HasOne(d => d.AdministrativeRegion).WithMany(p => p.Provinces)
                .HasForeignKey(d => d.AdministrativeRegionId)
                .HasConstraintName("provinces_administrative_region_id_fkey");

            entity.HasOne(d => d.AdministrativeUnit).WithMany(p => p.Provinces)
                .HasForeignKey(d => d.AdministrativeUnitId)
                .HasConstraintName("provinces_administrative_unit_id_fkey");
        });

        modelBuilder.Entity<Respondent>(entity =>
        {
            entity.HasKey(e => e.SbjNum).HasName("PRIMARY");

            entity.ToTable("respondents");

            entity.HasIndex(e => e.WardCode, "fk_res_add");

            entity.HasIndex(e => e.Marriage, "fk_res_mst");

            entity.Property(e => e.SbjNum).ValueGeneratedNever();
            entity.Property(e => e.AdressDetail)
                .HasMaxLength(100)
                .HasColumnName("adress_detail")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.EmailRes)
                .HasMaxLength(50)
                .HasColumnName("email_res")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Gender)
                .HasMaxLength(4)
                .HasColumnName("gender")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(100)
                .HasColumnName("household_income");
            entity.Property(e => e.IndivialIncome)
                .HasMaxLength(100)
                .HasColumnName("indivial_income");
            entity.Property(e => e.Job)
                .HasMaxLength(100)
                .HasColumnName("job")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Marriage)
                .HasMaxLength(100)
                .HasColumnName("marriage")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.NameRes)
                .HasMaxLength(100)
                .HasColumnName("name_res")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PathAdd)
                .HasMaxLength(100)
                .HasColumnName("path_add");
            entity.Property(e => e.PhoneRes)
                .HasMaxLength(30)
                .HasColumnName("phone_res");
            entity.Property(e => e.WardCode)
                .HasMaxLength(20)
                .HasColumnName("ward_code");
            entity.Property(e => e.YearOfBirth)
                .HasColumnType("year")
                .HasColumnName("year_of_birth");

            entity.HasOne(d => d.MarriageNavigation).WithMany(p => p.Respondents)
                .HasForeignKey(d => d.Marriage)
                .HasConstraintName("fk_res_mst");

            entity.HasOne(d => d.WardCodeNavigation).WithMany(p => p.Respondents)
                .HasForeignKey(d => d.WardCode)
                .HasConstraintName("fk_res_add");
        });

        modelBuilder.Entity<Typeproject>(entity =>
        {
            entity.HasKey(e => e.NameType).HasName("PRIMARY");

            entity.ToTable("typeproject");

            entity.Property(e => e.NameType)
                .HasMaxLength(100)
                .HasColumnName("name_type")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Ward>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PRIMARY");

            entity.ToTable("wards");

            entity.HasIndex(e => e.DistrictCode, "idx_wards_district");

            entity.HasIndex(e => e.AdministrativeUnitId, "idx_wards_unit");

            entity.HasIndex(e => e.NameEn, "name_en").HasAnnotation("MySql:FullTextIndex", true);

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasColumnName("code");
            entity.Property(e => e.AdministrativeUnitId).HasColumnName("administrative_unit_id");
            entity.Property(e => e.CodeName)
                .HasMaxLength(255)
                .HasColumnName("code_name");
            entity.Property(e => e.DistrictCode)
                .HasMaxLength(20)
                .HasColumnName("district_code");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(255)
                .HasColumnName("full_name_en");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NameEn).HasColumnName("name_en");

            entity.HasOne(d => d.AdministrativeUnit).WithMany(p => p.Wards)
                .HasForeignKey(d => d.AdministrativeUnitId)
                .HasConstraintName("wards_administrative_unit_id_fkey");

            entity.HasOne(d => d.DistrictCodeNavigation).WithMany(p => p.Wards)
                .HasForeignKey(d => d.DistrictCode)
                .HasConstraintName("wards_district_code_fkey");
        });

        modelBuilder.Entity<WelltrackHcp>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("welltrack_hcps");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<WelltrackPharmacist>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("welltrack_pharmacist");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        modelBuilder.Entity<_01Bigbang>(entity =>
        {
            entity.HasKey(e => e.Sbjnum).HasName("PRIMARY");

            entity.ToTable("01_bigbang");

            entity.Property(e => e.Sbjnum)
                .ValueGeneratedNever()
                .HasColumnName("SBJNUM");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Age).HasColumnName("AGE");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("CITY");
            entity.Property(e => e.Class).HasMaxLength(255);
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("CODE");
            entity.Property(e => e.ContactObject)
                .HasMaxLength(255)
                .HasColumnName("CONTACT_OBJECT");
            entity.Property(e => e.DateOfBirth).HasColumnName("DATE_OF_BIRTH");
            entity.Property(e => e.District)
                .HasMaxLength(255)
                .HasColumnName("DISTRICT");
            entity.Property(e => e.Education)
                .HasMaxLength(255)
                .HasColumnName("EDUCATION");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .HasColumnName("FULLNAME");
            entity.Property(e => e.HouseholdIncome)
                .HasMaxLength(50)
                .HasColumnName("HOUSEHOLD_INCOME");
            entity.Property(e => e.Job)
                .HasMaxLength(255)
                .HasColumnName("JOB");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(50)
                .HasColumnName("MARITAL_STATUS");
            entity.Property(e => e.MostFrequentlyUsedBrand)
                .HasMaxLength(255)
                .HasColumnName("MOST_FREQUENTLY_USED_BRAND");
            entity.Property(e => e.PersonalIncome)
                .HasMaxLength(50)
                .HasColumnName("PERSONAL_INCOME");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("PHONE_NUMBER");
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .HasColumnName("PROJECT_NAME");
            entity.Property(e => e.Provinces)
                .HasMaxLength(255)
                .HasColumnName("PROVINCES");
            entity.Property(e => e.Qa)
                .HasMaxLength(255)
                .HasColumnName("QA");
            entity.Property(e => e.Qc)
                .HasMaxLength(255)
                .HasColumnName("QC");
            entity.Property(e => e.Sex)
                .HasMaxLength(10)
                .HasColumnName("SEX");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("SOURCE");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("STREET");
            entity.Property(e => e.Stt).HasColumnName("STT");
            entity.Property(e => e.Ward)
                .HasMaxLength(255)
                .HasColumnName("WARD");
            entity.Property(e => e.Year).HasColumnName("YEAR");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
