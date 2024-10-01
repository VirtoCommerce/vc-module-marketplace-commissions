export declare class AuthApiBase {
    authToken: string;
    protected constructor();
    getBaseUrl(defaultUrl: string, baseUrl: string): string;
    setAuthToken(token: string): void;
    protected transformOptions(options: any): Promise<any>;
}
export declare class VcmpFeeClient extends AuthApiBase {
    private http;
    private baseUrl;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined;
    constructor(baseUrl?: string, http?: {
        fetch(url: RequestInfo, init?: RequestInit): Promise<Response>;
    });
    /**
     * @return OK
     */
    getNewFee(): Promise<DynamicCommissionFee>;
    protected processGetNewFee(response: Response): Promise<DynamicCommissionFee>;
    /**
     * @return OK
     */
    getFeeById(id: string): Promise<CommissionFee>;
    protected processGetFeeById(response: Response): Promise<CommissionFee>;
    /**
     * @param body (optional)
     * @return OK
     */
    createFee(body?: CreateFeeCommand | undefined): Promise<CommissionFee>;
    protected processCreateFee(response: Response): Promise<CommissionFee>;
    /**
     * @param body (optional)
     * @return OK
     */
    updateFee(body?: UpdateFeeCommand | undefined): Promise<CommissionFee>;
    protected processUpdateFee(response: Response): Promise<CommissionFee>;
    /**
     * @param ids (optional)
     * @return OK
     */
    deleteFee(ids?: string[] | undefined): Promise<void>;
    protected processDeleteFee(response: Response): Promise<void>;
    /**
     * @param body (optional)
     * @return OK
     */
    searchFee(body?: SearchCommissionFeesQuery | undefined): Promise<SearchCommissionFeesResult>;
    protected processSearchFee(response: Response): Promise<SearchCommissionFeesResult>;
}
export declare class VcmpSellerCommissionClient extends AuthApiBase {
    private http;
    private baseUrl;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined;
    constructor(baseUrl?: string, http?: {
        fetch(url: RequestInfo, init?: RequestInit): Promise<Response>;
    });
    /**
     * @return OK
     */
    getSellerCommission(sellerId: string): Promise<CommissionFee>;
    protected processGetSellerCommission(response: Response): Promise<CommissionFee>;
    /**
     * @param body (optional)
     * @return OK
     */
    updateSellerCommission(body?: UpdateSellerCommissionCommand | undefined): Promise<CommissionFee>;
    protected processUpdateSellerCommission(response: Response): Promise<CommissionFee>;
}
export declare class CommissionFee implements ICommissionFee {
    name?: string | undefined;
    description?: string | undefined;
    type?: CommissionFeeType2;
    calculationType?: CommissionFeeCalculationType;
    fee?: number;
    priority?: number;
    isDefault?: boolean;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: ICommissionFee);
    init(_data?: any): void;
    static fromJS(data: any): CommissionFee;
    toJSON(data?: any): any;
}
export interface ICommissionFee {
    name?: string | undefined;
    description?: string | undefined;
    type?: CommissionFeeType2;
    calculationType?: CommissionFeeCalculationType;
    fee?: number;
    priority?: number;
    isDefault?: boolean;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class CommissionFeeDetails implements ICommissionFeeDetails {
    id?: string | undefined;
    name?: string | undefined;
    description?: string | undefined;
    type?: CommissionFeeDetailsType;
    calculationType?: CommissionFeeDetailsCalculationType;
    fee?: number;
    priority?: number;
    isDefault?: boolean;
    isActive?: boolean;
    expressionTree?: DynamicCommissionFeeTree | undefined;
    constructor(data?: ICommissionFeeDetails);
    init(_data?: any): void;
    static fromJS(data: any): CommissionFeeDetails;
    toJSON(data?: any): any;
}
export interface ICommissionFeeDetails {
    id?: string | undefined;
    name?: string | undefined;
    description?: string | undefined;
    type?: CommissionFeeDetailsType;
    calculationType?: CommissionFeeDetailsCalculationType;
    fee?: number;
    priority?: number;
    isDefault?: boolean;
    isActive?: boolean;
    expressionTree?: DynamicCommissionFeeTree | undefined;
}
export declare enum CommissionFeeType {
    Static = "Static",
    Dynamic = "Dynamic"
}
export declare class CreateFeeCommand implements ICreateFeeCommand {
    feeDetails?: CommissionFeeDetails | undefined;
    constructor(data?: ICreateFeeCommand);
    init(_data?: any): void;
    static fromJS(data: any): CreateFeeCommand;
    toJSON(data?: any): any;
}
export interface ICreateFeeCommand {
    feeDetails?: CommissionFeeDetails | undefined;
}
export declare class DynamicCommissionFee implements IDynamicCommissionFee {
    isActive?: boolean;
    expressionTree?: DynamicCommissionFeeTree | undefined;
    name?: string | undefined;
    description?: string | undefined;
    type?: DynamicCommissionFeeType;
    calculationType?: DynamicCommissionFeeCalculationType;
    fee?: number;
    priority?: number;
    isDefault?: boolean;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
    constructor(data?: IDynamicCommissionFee);
    init(_data?: any): void;
    static fromJS(data: any): DynamicCommissionFee;
    toJSON(data?: any): any;
}
export interface IDynamicCommissionFee {
    isActive?: boolean;
    expressionTree?: DynamicCommissionFeeTree | undefined;
    name?: string | undefined;
    description?: string | undefined;
    type?: DynamicCommissionFeeType;
    calculationType?: DynamicCommissionFeeCalculationType;
    fee?: number;
    priority?: number;
    isDefault?: boolean;
    createdDate?: Date;
    modifiedDate?: Date | undefined;
    createdBy?: string | undefined;
    modifiedBy?: string | undefined;
    id?: string | undefined;
}
export declare class DynamicCommissionFeeTree implements IDynamicCommissionFeeTree {
    all?: boolean;
    not?: boolean;
    readonly id?: string | undefined;
    availableChildren?: IConditionTree[] | undefined;
    children?: IConditionTree[] | undefined;
    constructor(data?: IDynamicCommissionFeeTree);
    init(_data?: any): void;
    static fromJS(data: any): DynamicCommissionFeeTree;
    toJSON(data?: any): any;
}
export interface IDynamicCommissionFeeTree {
    all?: boolean;
    not?: boolean;
    id?: string | undefined;
    availableChildren?: IConditionTree[] | undefined;
    children?: IConditionTree[] | undefined;
}
export declare enum FeeCalculationType {
    Fixed = "Fixed",
    Percent = "Percent"
}
export declare class IConditionTree implements IIConditionTree {
    readonly id?: string | undefined;
    /** List of all available children for current tree node (is used in expression designer) */
    readonly availableChildren?: IConditionTree[] | undefined;
    readonly children?: IConditionTree[] | undefined;
    constructor(data?: IIConditionTree);
    init(_data?: any): void;
    static fromJS(data: any): IConditionTree;
    toJSON(data?: any): any;
}
export interface IIConditionTree {
    id?: string | undefined;
    /** List of all available children for current tree node (is used in expression designer) */
    availableChildren?: IConditionTree[] | undefined;
    children?: IConditionTree[] | undefined;
}
export declare class SearchCommissionFeesQuery implements ISearchCommissionFeesQuery {
    type?: CommissionFeeType | undefined;
    isDefault?: boolean | undefined;
    isActive?: boolean | undefined;
    responseGroup?: string | undefined;
    objectType?: string | undefined;
    objectTypes?: string[] | undefined;
    objectIds?: string[] | undefined;
    keyword?: string | undefined;
    searchPhrase?: string | undefined;
    languageCode?: string | undefined;
    sort?: string | undefined;
    readonly sortInfos?: SortInfo[] | undefined;
    skip?: number;
    take?: number;
    constructor(data?: ISearchCommissionFeesQuery);
    init(_data?: any): void;
    static fromJS(data: any): SearchCommissionFeesQuery;
    toJSON(data?: any): any;
}
export interface ISearchCommissionFeesQuery {
    type?: CommissionFeeType | undefined;
    isDefault?: boolean | undefined;
    isActive?: boolean | undefined;
    responseGroup?: string | undefined;
    objectType?: string | undefined;
    objectTypes?: string[] | undefined;
    objectIds?: string[] | undefined;
    keyword?: string | undefined;
    searchPhrase?: string | undefined;
    languageCode?: string | undefined;
    sort?: string | undefined;
    sortInfos?: SortInfo[] | undefined;
    skip?: number;
    take?: number;
}
export declare class SearchCommissionFeesResult implements ISearchCommissionFeesResult {
    totalCount?: number;
    results?: CommissionFee[] | undefined;
    constructor(data?: ISearchCommissionFeesResult);
    init(_data?: any): void;
    static fromJS(data: any): SearchCommissionFeesResult;
    toJSON(data?: any): any;
}
export interface ISearchCommissionFeesResult {
    totalCount?: number;
    results?: CommissionFee[] | undefined;
}
export declare enum SortDirection {
    Ascending = "Ascending",
    Descending = "Descending"
}
export declare class SortInfo implements ISortInfo {
    sortColumn?: string | undefined;
    sortDirection?: SortInfoSortDirection;
    constructor(data?: ISortInfo);
    init(_data?: any): void;
    static fromJS(data: any): SortInfo;
    toJSON(data?: any): any;
}
export interface ISortInfo {
    sortColumn?: string | undefined;
    sortDirection?: SortInfoSortDirection;
}
export declare class UpdateFeeCommand implements IUpdateFeeCommand {
    feeDetails?: CommissionFeeDetails | undefined;
    constructor(data?: IUpdateFeeCommand);
    init(_data?: any): void;
    static fromJS(data: any): UpdateFeeCommand;
    toJSON(data?: any): any;
}
export interface IUpdateFeeCommand {
    feeDetails?: CommissionFeeDetails | undefined;
}
export declare class UpdateSellerCommissionCommand implements IUpdateSellerCommissionCommand {
    sellerId?: string | undefined;
    sellerName?: string | undefined;
    commissionFeeId?: string | undefined;
    constructor(data?: IUpdateSellerCommissionCommand);
    init(_data?: any): void;
    static fromJS(data: any): UpdateSellerCommissionCommand;
    toJSON(data?: any): any;
}
export interface IUpdateSellerCommissionCommand {
    sellerId?: string | undefined;
    sellerName?: string | undefined;
    commissionFeeId?: string | undefined;
}
export declare enum CommissionFeeType2 {
    Static = "Static",
    Dynamic = "Dynamic"
}
export declare enum CommissionFeeCalculationType {
    Fixed = "Fixed",
    Percent = "Percent"
}
export declare enum CommissionFeeDetailsType {
    Static = "Static",
    Dynamic = "Dynamic"
}
export declare enum CommissionFeeDetailsCalculationType {
    Fixed = "Fixed",
    Percent = "Percent"
}
export declare enum DynamicCommissionFeeType {
    Static = "Static",
    Dynamic = "Dynamic"
}
export declare enum DynamicCommissionFeeCalculationType {
    Fixed = "Fixed",
    Percent = "Percent"
}
export declare enum SortInfoSortDirection {
    Ascending = "Ascending",
    Descending = "Descending"
}
export declare class ApiException extends Error {
    message: string;
    status: number;
    response: string;
    headers: {
        [key: string]: any;
    };
    result: any;
    constructor(message: string, status: number, response: string, headers: {
        [key: string]: any;
    }, result: any);
    protected isApiException: boolean;
    static isApiException(obj: any): obj is ApiException;
}
//# sourceMappingURL=virtocommerce.marketplacecommissions.d.ts.map