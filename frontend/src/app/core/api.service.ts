import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CashRegister, CocoaState, DashboardData, InventoryItem, InventoryLot, ProcessingBatch, Producer, PublicContent, PublicContentSection, PublicPrice, Purchase, ReceiptEmailResult, Sale, SettingsData } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = 'http://localhost:5080/api';

  publicPrice() { return this.http.get<PublicPrice>(`${this.base}/public/price`); }
  publicContent(section?: PublicContentSection) { return this.http.get<PublicContent[]>(`${this.base}/public-content`, { params: section ? { section } : {} }); }
  adminPublicContent() { return this.http.get<PublicContent[]>(`${this.base}/admin/public-content`); }
  createPublicContent(body: unknown) { return this.http.post<PublicContent>(`${this.base}/admin/public-content`, body); }
  updatePublicContent(id: string, body: unknown) { return this.http.put<PublicContent>(`${this.base}/admin/public-content/${id}`, body); }
  deletePublicContent(id: string) { return this.http.delete<void>(`${this.base}/admin/public-content/${id}`); }
  dashboard() { return this.http.get<DashboardData>(`${this.base}/dashboard`); }
  producers(search = '') { return this.http.get<Producer[]>(`${this.base}/producers`, { params: search ? { search } : {} }); }
  createProducer(body: unknown) { return this.http.post<Producer>(`${this.base}/producers`, body); }
  updateProducer(id: string, body: unknown) { return this.http.put<Producer>(`${this.base}/producers/${id}`, body); }
  deleteProducer(id: string) { return this.http.delete<void>(`${this.base}/producers/${id}`); }
  purchases() { return this.http.get<Purchase[]>(`${this.base}/purchases`); }
  createPurchase(body: unknown) { return this.http.post<Purchase>(`${this.base}/purchases`, body); }
  voidPurchase(id: string) { return this.http.delete<void>(`${this.base}/purchases/${id}`); }
  purchaseReceipt(id: string) { return this.http.get(`${this.base}/purchases/${id}/receipt`, { responseType: 'blob' }); }
  emailPurchaseReceipt(id: string, email?: string) { return this.http.post<ReceiptEmailResult>(`${this.base}/purchases/${id}/email-receipt`, { email: email || null }); }
  sales() { return this.http.get<Sale[]>(`${this.base}/sales`); }
  createSale(body: unknown) { return this.http.post<Sale>(`${this.base}/sales`, body); }
  voidSale(id: string) { return this.http.delete<void>(`${this.base}/sales/${id}`); }
  saleReceipt(id: string) { return this.http.get(`${this.base}/sales/${id}/receipt`, { responseType: 'blob' }); }
  emailSaleReceipt(id: string, email?: string) { return this.http.post<ReceiptEmailResult>(`${this.base}/sales/${id}/email-receipt`, { email: email || null }); }
  inventory() { return this.http.get<InventoryItem[]>(`${this.base}/inventory`); }
  inventoryLots(state?: CocoaState) { return this.http.get<InventoryLot[]>(`${this.base}/inventory/lots`, { params: state ? { state } : {} }); }
  currentCashRegister() { return this.http.get<CashRegister | null>(`${this.base}/cash-registers/current`); }
  cashRegisters() { return this.http.get<CashRegister[]>(`${this.base}/cash-registers`); }
  openCashRegister(body: unknown) { return this.http.post<CashRegister>(`${this.base}/cash-registers/open`, body); }
  addCashMovement(id: string, body: unknown) { return this.http.post<CashRegister>(`${this.base}/cash-registers/${id}/movements`, body); }
  closeCashRegister(id: string, body: unknown) { return this.http.post<CashRegister>(`${this.base}/cash-registers/${id}/close`, body); }
  processing() { return this.http.get<ProcessingBatch[]>(`${this.base}/processing`); }
  createProcessing(body: unknown) { return this.http.post<ProcessingBatch>(`${this.base}/processing`, body); }
  completeProcessing(id: string, body: unknown) { return this.http.post<ProcessingBatch>(`${this.base}/processing/${id}/complete`, body); }
  cancelProcessing(id: string) { return this.http.post<ProcessingBatch>(`${this.base}/processing/${id}/cancel`, {}); }
  settings() { return this.http.get<SettingsData>(`${this.base}/settings`); }
  updateSettings(body: unknown) { return this.http.put<SettingsData>(`${this.base}/settings`, body); }
  refreshPrice() { return this.http.post<{ updated: boolean; message: string }>(`${this.base}/settings/refresh-price`, {}); }
  login(email: string, password: string) { return this.http.post<{ token: string; expiresAtUtc: string; fullName: string; email: string }>(`${this.base}/auth/login`, { email, password }); }
}
