import { Tree, TreeItem, TreeItemGroup } from '@angular/aria/tree';
import { CdkMenu, CdkMenuItem, CdkMenuTrigger } from '@angular/cdk/menu';
import { Component, input, output, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { DistrictNode } from '../../models/district.model';
import { PermissionDirective } from '../../services/auth/permission.directive';
import { NgTemplateOutlet } from '@angular/common';

@Component({
  selector: 'app-district-tree-ui',
  imports: [
    Tree,
    TreeItem,
    TreeItemGroup,
    CdkMenuTrigger,
    CdkMenu,
    CdkMenuItem,
    NgTemplateOutlet,
    PermissionDirective,
    TranslatePipe,
  ],
  templateUrl: './district-tree-ui.html',
  styleUrl: './district-tree-ui.scss',
})
export class DistrictTreeUi {
  nodes = input.required<DistrictNode[]>();
  initialSelectedId = input<number | undefined>(undefined);

  // Outputs pour les actions du menu
  nodeSelected = output<DistrictNode>();
  editNode = output<DistrictNode>();
  deleteNode = output<DistrictNode>();
  toggleStatus = output<DistrictNode>();

  protected readonly selectedIds = signal<number[]>([]);

  isLeaf(node: DistrictNode): boolean {
    return (
      node.children === undefined || node.children.length === 0 //&& node.level === 'votingLocation'
    );
  }
  //Gère le changement de sélection dans l'arborescence
  protected onSelectionChange(ids: number[]): void {
    if (ids.length === 0) return;

    const nodeid = ids[0];
    let node = this.findNodeById(this.nodes(), nodeid);

    if (node && this.isLeaf(node)) {
      this.selectedIds.set(ids);
      this.nodeSelected.emit(node);
    } else {
      node = undefined;
    }
  }

  //Recherche récursive optimisée
  private findNodeById(nodes: DistrictNode[], id: number): DistrictNode | undefined {
    for (const node of nodes) {
      if (node.id === id) return node;

      if (node.children?.length) {
        const found = this.findNodeById(node.children, id);
        if (found) return found;
      }
    }
    return undefined;
  }

  // Actions du menu
  onEdit(node: DistrictNode): void {
    this.editNode.emit(node);
  }
  onDelete(node: DistrictNode): void {
    this.deleteNode.emit(node);
  }
  onToggleStatus(node: DistrictNode): void {
    this.toggleStatus.emit(node);
  }
}
