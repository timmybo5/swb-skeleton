﻿using System.Collections.Generic;
using Sandbox;

/* 
 * Weapon base for weapons using magazine based reloading 
*/

namespace SWB_Base;

public partial class WeaponBase
{
    [Net] public IList<ActiveAttachment> ActiveAttachments { get; set; }
    private bool initializedAttachments = false;

    protected void InitAttachments()
    {
        Game.AssertServer();

        foreach (var attachmentCategory in AttachmentCategories)
        {
            foreach (var attachment in attachmentCategory.Attachments)
            {
                // Add forced attachments
                if (attachment.Enabled)
                {
                    var activeAttachment = new ActiveAttachment
                    {
                        Name = attachment.Name,
                        Category = attachmentCategory.Name,
                        Forced = true,
                    };

                    // Adding to ActiveAttachments on client will not work since server overrides it
                    ActiveAttachments.Add(activeAttachment);

                    if (!string.IsNullOrEmpty(attachment.RequiresAttachmentWithName))
                    {
                        var reqAttachment = new ActiveAttachment
                        {
                            Name = attachment.RequiresAttachmentWithName,
                            Category = GetAttachmentCategoryName(attachment.RequiresAttachmentWithName),
                            Forced = true,
                        };

                        ActiveAttachments.Add(reqAttachment);
                    }

                    // One attachment per category
                    break;
                }
            }
        }
    }

    public AttachmentBase GetAttachment(string name)
    {
        foreach (var attachmentCategory in AttachmentCategories)
        {
            foreach (var attachment in attachmentCategory.Attachments)
            {
                if (attachment.Name == name)
                {
                    return attachment;
                }

            }
        }

        return null;
    }

    public ActiveAttachment GetActiveAttachment(string name)
    {
        foreach (var activeAttachment in ActiveAttachments)
        {
            if (activeAttachment.Name == name)
            {
                return activeAttachment;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds an AttachmentCategory by name
    /// </summary>
    public AttachmentCategory GetAttachmentCategory(string name)
    {
        foreach (var attachmentCategory in AttachmentCategories)
        {
            if (attachmentCategory.Name.ToString() == name)
            {
                return attachmentCategory;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets an AttachmentCategoryName from an attachment name
    /// </summary>
    public AttachmentCategoryName GetAttachmentCategoryName(string name)
    {
        foreach (var attachmentCategory in AttachmentCategories)
        {
            foreach (var attachment in attachmentCategory.Attachments)
            {
                if (attachment.Name == name)
                {
                    return attachmentCategory.Name;
                }
            }
        }
        return AttachmentCategoryName.None;
    }

    /// <summary>
    /// Equips an attachment and assigns a valid attachment model to the activeAttachment.
    /// Gets called by EquipAttachmentSV for both client and server.
    /// </summary>
    public void EquipAttachment(ActiveAttachment activeAttachment)
    {
        var attachment = GetAttachment(activeAttachment.Name);
        if (attachment == null)
        {
            LogUtil.Error("Attachment with name " + activeAttachment.Name + " does not exist!");
            return;
        }

        var shouldCreateModel = Game.IsServer ? activeAttachment.WorldAttachmentModel == null : true;
        var attachmentModel = attachment.Equip(this, shouldCreateModel);

        if (attachmentModel != null)
        {
            if (Game.IsClient)
            {
                activeAttachment.ViewAttachmentModel = attachmentModel;
            }
            else
            {
                activeAttachment.WorldAttachmentModel = attachmentModel;
            }
        }

        if (!ActiveAttachments.Contains(activeAttachment))
        {
            ActiveAttachments.Add(activeAttachment);
        }
    }

    /// <summary>
    /// Creates an ActiveAttachment and calls EquipAttachment.
    /// </summary>
    public void EquipAttachment(string name)
    {
        var activeAttachment = new ActiveAttachment
        {
            Name = name,
            Category = GetAttachmentCategoryName(name)
        };

        ActiveAttachments.Add(activeAttachment);

        EquipAttachment(activeAttachment);
    }

    /// <summary>
    /// Unequips an attachment.
    /// Gets called by EquipAttachmentSV for both client and server.
    /// </summary>
    public void UnequipAttachment(string name)
    {
        var activeAttachment = GetActiveAttachment(name);

        if (activeAttachment != null)
        {
            ActiveAttachments.Remove(activeAttachment);
        }

        var attachment = GetAttachment(name);
        attachment.Unequip(this);
    }

    [ClientRpc]
    public void UnequipAttachmentCL(string name)
    {
        UnequipAttachment(name);
    }

    /// <summary>
    /// Tries to equip attachment on client after server created a valid networked attachment
    /// </summary>
    [ClientRpc]
    protected void EquipAttachmentCL(string name)
    {
        // Handle EquipBeforeActive, will be initialized by HandleAttachments when WeaponBase.ActiveStart gets called
        if (InitialStats == null)
        {
            var activeAtt = new ActiveAttachment
            {
                Name = name,
                Category = GetAttachmentCategoryName(name),
            };

            ActiveAttachments.Add(activeAtt);
            return;
        }

        var instanceID = InstanceID;
        TryEquipAttachmentCL(name, instanceID);
    }

    protected async void TryEquipAttachmentCL(string name, int instanceID)
    {
        Game.AssertClient();

        var activeAttachment = GetActiveAttachment(name);

        if (activeAttachment != null)
        {
            EquipAttachment(activeAttachment);
            return;
        }

        var player = Owner as ISWBPlayer;
        var activeWeapon = player.ActiveChild;
        await GameTask.DelaySeconds(0.05f);
        if (!IsAsyncValid(activeWeapon, instanceID)) return;
        TryEquipAttachmentCL(name, instanceID);
    }

    /// <summary>
    /// Toggle an attachment's required attachment
    /// </summary>
    public void ToggleRequiredAttachment(string name, bool toggle)
    {
        var attach = GetAttachment(name);

        if (string.IsNullOrEmpty(attach.RequiresAttachmentWithName)) return;

        var activeAttach = GetActiveAttachment(attach.RequiresAttachmentWithName);
        if (toggle && activeAttach == null)
        {
            EquipAttachmentSV(attach.RequiresAttachmentWithName);
        }
        else if (!toggle && activeAttach != null)
        {
            // Check if another attachment is using it
            foreach (var cat in AttachmentCategories)
            {
                foreach (var activeAtt in ActiveAttachments)
                {
                    // Do not check current attachment
                    if (activeAtt.Name == name) continue;

                    var checkingAtt = GetAttachment(activeAtt.Name);

                    if (checkingAtt.RequiresAttachmentWithName == attach.RequiresAttachmentWithName)
                    {
                        // Do not unequip if used by other attachment
                        return;
                    }
                }
            }

            UnequipAttachmentSV(attach.RequiresAttachmentWithName);
        }
    }

    /// <summary>
    /// Request server to equip an attachment.
    /// </summary>
    public async void EquipAttachmentSV(string name)
    {
        // HandleAttachments should take care of attachment if equipped before ActiveStart
        if (InitialStats == null)
        {
            var attach = GetAttachment(name);
            attach.EquipBeforeActive = true;
            return;
        }

        // Unequip same category attachment
        var cat = GetAttachmentCategoryName(name);
        var catAtt = GetActiveAttachmentFromCategory(cat);
        if (catAtt != null)
        {
            UnequipAttachmentSV(catAtt.Name);

            // Networked list bug workaround, changing the list too fast will corrupt it on client
            await GameTask.DelaySeconds(0.01f);
        }

        ToggleRequiredAttachment(name, true);

        // Server
        EquipAttachment(name);

        // Client
        EquipAttachmentCL(To.Single(Owner), name);
    }

    /// <summary>
    /// Request server to unequip an attachment.
    /// </summary>
    public void UnequipAttachmentSV(string name)
    {
        ToggleRequiredAttachment(name, false);

        // Server
        UnequipAttachment(name);

        // Client
        UnequipAttachmentCL(To.Single(Owner), name);
    }

    [ClientRpc]
    protected void HandleAttachmentsRecheckCL()
    {
        var instanceID = InstanceID;
        TryHandleAttachmentsCL(instanceID);
    }

    protected async void TryHandleAttachmentsCL(int instanceID)
    {
        Game.AssertClient();

        if (ActiveAttachments.Count > 0)
        {
            HandleAttachments(true);
            return;
        }

        var player = Owner as ISWBPlayer;
        var activeWeapon = player.ActiveChild;

        await GameTask.DelaySeconds(0.05f);
        if (!IsAsyncValid(activeWeapon, instanceID)) return;
        TryHandleAttachmentsCL(instanceID);
    }

    /// <summary>
    /// Handles forced and active attachments.
    /// </summary>
    public virtual void HandleAttachments(bool shouldEquip)
    {
        if (AttachmentCategories != null)
        {
            // Initialize attachments on server
            if (Game.IsServer && shouldEquip && !initializedAttachments)
            {
                initializedAttachments = true;
                InitAttachments();

                // Client will not have attachments instantly
                if (ActiveAttachments.Count > 0)
                {
                    HandleAttachmentsRecheckCL(To.Single(Owner));
                }
            }

            foreach (var activeAttachment in ActiveAttachments)
            {
                var attachment = GetAttachment(activeAttachment.Name);

                if (attachment == null) continue;

                if (shouldEquip)
                {
                    EquipAttachment(activeAttachment);
                }
                else
                {
                    attachment.Unequip(this);
                }
            }

            // Handle EquipBeforeActive
            if (Game.IsServer)
            {
                foreach (var attachmentCategory in AttachmentCategories)
                {
                    foreach (var attachment in attachmentCategory.Attachments)
                    {
                        if (attachment.EquipBeforeActive)
                        {
                            attachment.EquipBeforeActive = false;
                            EquipAttachmentSV(attachment.Name);

                            // Only 1 attachment per category
                            break;
                        }
                    }
                }
            }
        }
    }

    public virtual ActiveAttachment GetActiveAttachmentFromCategory(AttachmentCategoryName attachmentCategoryName)
    {
        return GetActiveAttachmentFromCategory(attachmentCategoryName.ToString());
    }

    public virtual ActiveAttachment GetActiveAttachmentFromCategory(string attachmentCategoryName)
    {
        if (AttachmentCategories != null)
        {
            foreach (var activeAttachment in ActiveAttachments)
            {
                if (activeAttachment.Category.ToString() == attachmentCategoryName)
                {
                    return activeAttachment;
                }
            }
        }

        return null;
    }
}
