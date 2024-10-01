import { OverridesSchema } from "@vc-shell/framework";

export const overrides: OverridesSchema = {
  upsert: [
    {
      id: "SellerDetails",
      path: "content.sellerDetailsForm.children.mainSellerFieldset.fields.basicInfoCard.fields",
      index: 1,
      value: {
        id: "commissionFee",
        component: "vc-field",
        label: "COMMISSIONS.CONTROL.COMMISSION.LABEL",
        variant: "text",
        property: "computedFee",
      },
    },
  ],
};
