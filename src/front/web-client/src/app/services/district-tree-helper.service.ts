import { computed, inject, Injectable, resource, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DistrictNode } from '../models/district.model';
import { DistrictApiService } from './api/district.api.service';
import { GetDistrictsResponse } from './nswag/api-nswag-client';

@Injectable({
  providedIn: 'root',
})
export class DistrictTreeHelperService {
  private readonly districtService = inject(DistrictApiService);

  private readonly _selectedNode = signal<DistrictNode | null>(null);
  readonly selectedNode = this._selectedNode.asReadonly();

  private readonly districtsResource = resource({
    loader: async () => {
      const res = await firstValueFrom(this.districtService.getDistricts({}));
      return res?.map((c) => this.mapToNode(c)) ?? [];
    },
  });

  readonly nodesData = computed(() => this.districtsResource.value() ?? []);
  readonly isLoading = this.districtsResource.isLoading;

  reload(): void {
    this.districtsResource.reload();
  }

  findNode(id: number): DistrictNode | undefined {
    return this.findNodeInTree(this.nodesData(), id);
  }

  setSelectedNode(node: DistrictNode | number | undefined): void {
    if (typeof node === 'number') {
      const foundNode = this.findNodeInTree(this.nodesData(), node);
      this._selectedNode.set(foundNode ?? null);
    } else {
      this._selectedNode.set(node ?? null);
    }
  }

  private findNodeInTree(nodes: DistrictNode[], targetId: number): DistrictNode | undefined {
    for (const node of nodes) {
      if (node.id === targetId) return node;
      if (node.children?.length) {
        const found = this.findNodeInTree(node.children, targetId);
        if (found) return found;
      }
    }
    return undefined;
  }

  private mapToNode(district: GetDistrictsResponse): DistrictNode {
    return {
      id: district.id!,
      code: district.code!,
      wording: district.wording!,
      level: district.level!,
      active: district.isActive ?? false,
      parentId: district.parentId!,
      children: district.children?.map((d) => this.mapToNode(d)),
    };
  }

  findChildren(nodes: DistrictNode[], parentId: number | null): DistrictNode[] {
    if (!parentId) return [];
    return nodes.find((n) => n.id === parentId)?.children ?? [];
  }
}
