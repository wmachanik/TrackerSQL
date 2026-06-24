# TrackerDb to TrackerSQLDb Migration - Progress Tracker

**Started:** [Date]  
**Target Completion:** [Date + 3 weeks]  
**Last Updated:** 2025-05-14

---

## ?? Overall Progress

| Metric | Count | Status |
|--------|-------|--------|
| **Total Files** | 66 | |
| **Completed** | 0 | 0% |
| **In Progress** | 0 | |
| **Not Started** | 66 | 100% |
| **Total Usages** | 1,746 | |
| **Migrated** | 0 | 0% |

---

## ?? Phase 1: Critical Path (Week 1)

**Target:** 4 files, ~16 hours  
**Status:** Not Started

| File | Usages | Status | Started | Completed | Notes |
|------|--------|--------|---------|-----------|-------|
| `Controls\CustomersTbl.cs` | 168 | ? Not Started | | | |
| `Controls\ItemTypeTbl.cs` | 99 | ? Not Started | | | |
| `Controls\OrderDataControl.cs` | 14 | ? Not Started | | | |
| *(Practice file)* | varies | ? Not Started | | | |

**Checklist:**
- [ ] Practice file completed
- [ ] All Phase 1 files migrated
- [ ] Unit tests passing
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Changes deployed to test environment
- [ ] Regression testing completed

---

## ?? Phase 2: High Impact (Week 2)

**Target:** 5 files, ~15 hours  
**Status:** Not Started

| File | Usages | Status | Started | Completed | Notes |
|------|--------|--------|---------|-----------|-------|
| `Controls\ReoccuringOrderDAL.cs` | 61 | ? Not Started | | | |
| `Controls\PersonsTbl.cs` | 49 | ? Not Started | | | |
| `Controls\ItemUsageTbl.cs` | 45 | ? Not Started | | | |
| `Controls\ClientUsageTbl.cs` | 42 | ? Not Started | | | |
| `Controls\OrderDetailDAL.cs` | 36 | ? Not Started | | | |

**Checklist:**
- [ ] All Phase 2 files migrated
- [ ] Unit tests passing
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Changes deployed to test environment
- [ ] Regression testing completed

---

## ?? Phase 3: Supporting Tables (Week 3, Days 1-3)

**Target:** 20 files, ~40 hours  
**Status:** Not Started

### Table Classes (High Priority)
| File | Usages | Status | Completed | Notes |
|------|--------|--------|-----------|-------|
| `Controls\TempOrdersDAL.cs` | 35 | ? | | |
| `Controls\CustomersAccInfoTbl.cs` | 34 | ? | | |
| `Controls\ClientUsageLinesTbl.cs` | 33 | ? | | |
| `Controls\EquipTypeTbl.cs` | 33 | ? | | |
| `Controls\TempCoffeeCheckup.cs` | 31 | ? | | |
| `Controls\NextPrepDateByAreaTbl.cs` | 31 | ? | | |
| `Controls\RepairsTbl.cs` | 29 | ? | | |
| `Controls\MachineConditionsTbl.cs` | 28 | ? | | |
| `Controls\SysDataTbl.cs` | 26 | ? | | |
| `Controls\LogTbl.cs` | 26 | ? | | |

### Table Classes (Medium Priority)
| File | Usages | Status | Completed | Notes |
|------|--------|--------|-----------|-------|
| `Controls\InvoiceTypeTbl.cs` | 25 | ? | | |
| `Controls\OrderItemTbl.cs` | 25 | ? | | |
| `Controls\SectionTypesTbl.cs` | 24 | ? | | |
| `Controls\OrderCheckTbl.cs` | 24 | ? | | |
| `Controls\TransactionTypesTbl.cs` | 23 | ? | | |
| `Controls\TempOrdersLinesTbl.cs` | 23 | ? | | |
| `Controls\AreaPrepDaysTbl.cs` | 23 | ? | | Example file |
| `Controls\PackagingTbl.cs` | 22 | ? | | |
| `Controls\PaymentTermsTbl.cs` | 21 | ? | | |
| `Controls\SentRemindersLogTbl.cs` | 20 | ? | | |

**Checklist:**
- [ ] All Phase 3 files migrated
- [ ] Unit tests passing
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Changes deployed to test environment
- [ ] Regression testing completed

---

## ?? Phase 4: Lookup Tables (Week 3, Days 4-5)

**Target:** 20+ files, ~20 hours  
**Status:** Not Started

### DAL and Other Files
| File | Usages | Status | Completed | Notes |
|------|--------|--------|-----------|-------|
| `Controls\RepairStatusesTbl.cs` | 19 | ? | | |
| `Controls\TempOrdersHeaderTbl.cs` | 18 | ? | | |
| `Controls\CustomerTypeTbl.cs` | 17 | ? | | |
| `Controls\ItemGroupTbl.cs` | 16 | ? | | |
| `Controls\OrderCls.cs` | 16 | ? | | |
| `Controls\PrepTypesTbl.cs` | 16 | ? | | |
| `Controls\OrderDetailData.cs` | 14 | ? | | |
| `Controls\PriceLevelsTbl.cs` | 14 | ? | | |
| `Controls\ItemContactRequires.cs` | 14 | ? | | |
| `Controls\AreaTblDAL.cs` | 13 | ? | | |
| `Controls\HolidayClosureProvider.cs` | 9 | ? | | |
| `Controls\CustomerTrackedServiceItems.cs` | 9 | ? | | |
| `Controls\OrderCheck.cs` | 8 | ? | | |
| `Controls\ActiveDeliveryData.cs` | 8 | ? | | |
| `Controls\ContactsThatMayNeedNextWeek.cs` | 7 | ? | | |
| `Controls\ClientAwayPeriod.cs` | 7 | ? | | |

**Checklist:**
- [ ] All Phase 4 files migrated
- [ ] Unit tests passing
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Changes deployed to test environment
- [ ] Regression testing completed

---

## ?? Other Categories

### Pages (Code-Behind)
| File | Usages | Status | Completed | Notes |
|------|--------|--------|-----------|-------|
| `Pages\DeliverySheet.aspx.cs` | 12 | ? | | Consider repository pattern |
| `Pages\ViewMyOrder.aspx.cs` | 11 | ? | | Consider repository pattern |
| `Pages\NewOrderDetail.aspx.cs` | 6 | ? | | Consider repository pattern |
| `Pages\DeleteOrderLine.aspx.cs` | 4 | ? | | Consider repository pattern |

### Classes (Utilities)
| File | Usages | Status | Completed | Notes |
|------|--------|--------|-----------|-------|
| `Classes\GeneralTrackerDbTools.cs` | 13 | ? | | |
| `Classes\TrackerTools.cs` | 9 | ? | | |
| `Classes\DateMatrixBuilder.cs` | 9 | ? | | |

### Other
| File | Usages | Status | Completed | Notes |
|------|--------|--------|-----------|-------|
| `test\ShowTableStruct.aspx.cs` | 9 | ? | | Low priority - test file |
| `Tools\XMLtoSQL.aspx.cs` | 4 | ? | | |
| `Managers\CoffeeCheckupManager.cs` | 4 | ? | | |

---

## ?? Weekly Progress Summary

### Week 1 (Target: Phase 1 Complete)
- **Files Migrated:** 0 / 4
- **Methods Migrated:** 0 / ~50
- **Tests Passing:** 0 / 4 files
- **Blockers:** None
- **Notes:**

### Week 2 (Target: Phase 2 Complete)
- **Files Migrated:** 0 / 5
- **Methods Migrated:** 0 / ~60
- **Tests Passing:** 0 / 5 files
- **Blockers:** None
- **Notes:**

### Week 3 (Target: Phase 3 & 4 Complete)
- **Files Migrated:** 0 / 40+
- **Methods Migrated:** 0 / ~200+
- **Tests Passing:** 0 / 40+ files
- **Blockers:** None
- **Notes:**

---

## ?? Issues & Blockers

### Open Issues
| Issue # | File | Description | Status | Resolution |
|---------|------|-------------|--------|------------|
| | | | | |

### Resolved Issues
| Issue # | File | Description | Resolution | Date |
|---------|------|-------------|------------|------|
| | | | | |

---

## ?? Lessons Learned

### What Worked Well
- 

### What Could Be Improved
- 

### Best Practices Identified
- 

---

## ? Final Checklist

### Pre-Production
- [ ] All 66 files migrated
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] Full regression testing complete
- [ ] Performance testing complete
- [ ] Code review complete
- [ ] Documentation updated
- [ ] Migration Plan marked 100% complete

### Production Deployment
- [ ] Deployment plan reviewed
- [ ] Rollback plan documented
- [ ] Database backup taken
- [ ] Deploy to staging
- [ ] Staging testing complete
- [ ] Deploy to production
- [ ] Production smoke tests complete
- [ ] Monitoring in place

### Post-Production
- [ ] Monitor for 24 hours
- [ ] Verify logs
- [ ] Check performance metrics
- [ ] User acceptance testing
- [ ] Close migration project
- [ ] Archive old TrackerDb class
- [ ] Celebrate success! ??

---

## ?? Metrics Tracking

### Code Quality
- **Before Migration:**
  - Lines of Code: [TBD]
  - Cyclomatic Complexity: [TBD]
  - Test Coverage: [TBD]%

- **After Migration:**
  - Lines of Code: [TBD]
  - Cyclomatic Complexity: [TBD]
  - Test Coverage: [TBD]%

### Performance
- **Before Migration:**
  - Avg Query Time: [TBD]ms
  - Page Load Time: [TBD]ms
  - Database Connections: [TBD]

- **After Migration:**
  - Avg Query Time: [TBD]ms
  - Page Load Time: [TBD]ms
  - Database Connections: [TBD]

---

## ?? Daily Standup Format

**Date:** [Date]  
**Developer:** [Name]

**Yesterday:**
- Migrated: [File names]
- Completed: [X] methods
- Tested: [Y] methods

**Today:**
- Planning to migrate: [File names]
- Expected completion: [X] methods

**Blockers:**
- [None / List blockers]

**Help Needed:**
- [None / List items]

---

## Version History

| Version | Date | Changes | Updated By |
|---------|------|---------|------------|
| 1.0 | 2025-05-14 | Initial tracker created | System |
| | | | |

---

**Update this tracker daily to maintain accurate progress visibility!**

**Status Key:**
- ? Not Started
- ?? In Progress
- ? Completed
- ? Blocked
- ?? On Hold
